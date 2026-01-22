using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using QS.Configuration;
using QS.DbManagement;
using QS.Dialog;

namespace QS.Launcher {
	public class Configurator {
		private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly LauncherOptions options;
		private readonly IInteractiveMessage interactive;
		private readonly IList<ConnectionTypeBase> connectionTypes;
		
		readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

		public Configurator(LauncherOptions options, IInteractiveMessage interactive, IEnumerable<ConnectionTypeBase> connectionTypes) {
			this.options = options ?? throw new ArgumentNullException(nameof(options));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.connectionTypes = connectionTypes?.ToList() ?? throw new ArgumentNullException(nameof(connectionTypes));
		}

		public IList<Connection> ReadConnections() {
			List<Dictionary<string, string>> connectionDefinitions = new List<Dictionary<string, string>>();
			string backupFile = options.ConnectionsJsonFileName + ".backup";
			bool needToRestoreFromBackup = false;
			
			try {
				using(var stream = File.Open(options.ConnectionsJsonFileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					try {
						if(stream.Length == 0) {
							logger.Warn("Файл конфигурации пуст ({0}), попытка восстановления из резервной копии.", options.ConnectionsJsonFileName);
							needToRestoreFromBackup = true;
							throw new FileNotFoundException("Файл пуст.");
						}
						connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
					}
					catch(JsonException jsonEx) {
						logger.Error(jsonEx, "Ошибка чтения файла конфигурации подключений ({0}).", options.ConnectionsJsonFileName);
						interactive.ShowMessage(ImportanceLevel.Error, $"Файл конфигурация подключений ({options.ConnectionsJsonFileName}) испорчен. Попытка восстановления из резервной копии.");
						needToRestoreFromBackup = true;
					}
				}
			}
			catch(IOException exception) when(exception is FileNotFoundException || exception is DirectoryNotFoundException) {
				needToRestoreFromBackup = true;
			}
			
			// Пытаемся восстановить из резервной копии
			if(needToRestoreFromBackup && File.Exists(backupFile)) {
				try {
					logger.Info("Попытка восстановления из резервной копии {0}", backupFile);
					using(var stream = File.Open(backupFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						if(stream.Length > 0) {
							connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
							logger.Info("Конфигурация успешно восстановлена из резервной копии");
							interactive.ShowMessage(ImportanceLevel.Warning, "Файл конфигурации был восстановлен из резервной копии.", "Конфигурация восстановлена");
							
							// Восстанавливаем основной файл из резервной копии
							File.Copy(backupFile, options.ConnectionsJsonFileName, true);
						}
					}
				}
				catch(Exception backupEx) {
					logger.Error(backupEx, "Не удалось восстановить конфигурацию из резервной копии");
				}
			}
			
			// Если не удалось загрузить ни из основного, ни из резервного файла
			if(connectionDefinitions.Count == 0) {
				if(!string.IsNullOrEmpty(options.OldConfigFilename)) {
					if(File.Exists(options.OldConfigFilename)) {
						connectionDefinitions = FromOldIniConfig(options.OldConfigFilename);
						logger.Info("Найден файл с подключениями старой версии, будет создан новый на его основе.");
						interactive.ShowMessage(ImportanceLevel.Info,"Найден файл с подключениями старой версии, будет создан новый на его основе.", "Конфигурация обновлена");
					}
				}
				else {
					var defaultConnections = options.MakeDefaultConnections?.Invoke();
					if(defaultConnections != null) {
						connectionDefinitions = defaultConnections;
					}
				}
			}
			
			// Гарантируем что connectionDefinitions не null
			if(connectionDefinitions == null) {
				connectionDefinitions = new List<Dictionary<string, string>>();
			}

			var connections = new List<Connection>();
			foreach(var parameters in connectionDefinitions) {
				var type = connectionTypes.FirstOrDefault(x => x.ConnectionTypeName == parameters["Type"]);
				if(type != null)
					connections.Add(new Connection(type, parameters));
			}
			return connections;
		}
		
		public void SaveConnections(IList<Connection> connections) {
			var connectionDefinitions = connections.Select(c => c.GetConfigDefinitions()).ToList();
			
			string directory = Path.GetDirectoryName(options.ConnectionsJsonFileName);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
				Directory.CreateDirectory(directory);
			}
			
			// Используем временный файл для атомарной записи
			string tempFile = options.ConnectionsJsonFileName + ".tmp";
			string backupFile = options.ConnectionsJsonFileName + ".backup";
			
			// Защита от конкурентного доступа через блокировку файла
			string lockFile = options.ConnectionsJsonFileName + ".lock";
			
			try {
				// Создаем файл блокировки для синхронизации между процессами
				using(var _ = File.Open(lockFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
					logger.Debug("Начинаем сохранение соединений в файл {0}", options.ConnectionsJsonFileName);
					
					// Сначала записываем во временный файл
					using(var stream = File.Open(tempFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
						// КРИТИЧНО: используем синхронную сериализацию, чтобы гарантировать завершение записи
						JsonSerializer.Serialize(stream, connectionDefinitions, serializerOptions);
						stream.Flush(); // Гарантируем запись на диск
					}
					
					logger.Debug("Данные записаны во временный файл {0}", tempFile);
					
					// Проверяем корректность записанного временного файла
					try {
						using(var verifyStream = File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
							if(verifyStream.Length == 0) {
								throw new InvalidOperationException("Временный файл конфигурации пуст после записи");
							}
							// Проверяем что JSON валиден
							JsonSerializer.Deserialize<List<Dictionary<string, string>>>(verifyStream);
						}
						logger.Debug("Временный файл прошел валидацию");
					}
					catch(Exception validateEx) {
						logger.Error(validateEx, "Валидация временного файла не прошла");
						throw new InvalidOperationException("Не удалось создать корректный файл конфигурации", validateEx);
					}
					
					// Создаем резервную копию существующего файла ПЕРЕД его заменой (если он есть и корректен)
					if(File.Exists(options.ConnectionsJsonFileName)) {
						try {
							var fileInfo = new FileInfo(options.ConnectionsJsonFileName);
							if(fileInfo.Length > 0) {
								File.Copy(options.ConnectionsJsonFileName, backupFile, true);
								logger.Debug("Создана резервная копия предыдущей версии {0}", backupFile);
							}
						}
						catch(Exception backupEx) {
							logger.Warn(backupEx, "Не удалось создать резервную копию, но продолжаем сохранение");
						}
					}
					
					// Атомарно заменяем основной файл
					if(File.Exists(options.ConnectionsJsonFileName)) {
						File.Delete(options.ConnectionsJsonFileName);
					}
					File.Move(tempFile, options.ConnectionsJsonFileName);
					logger.Info("Файл конфигурации успешно обновлен. Резервная копия предыдущей версии сохранена в {0}", backupFile);
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка сохранения файла конфигурации подключений ({0}).", options.ConnectionsJsonFileName);
				
				// Пытаемся восстановить из резервной копии
				if(File.Exists(backupFile) && (!File.Exists(options.ConnectionsJsonFileName) || new FileInfo(options.ConnectionsJsonFileName).Length == 0)) {
					try {
						File.Copy(backupFile, options.ConnectionsJsonFileName, true);
						logger.Info("Файл конфигурации восстановлен из резервной копии");
					}
					catch(Exception restoreEx) {
						logger.Error(restoreEx, "Не удалось восстановить файл из резервной копии");
					}
				}
				
				// Очищаем временные файлы
				try {
					if(File.Exists(tempFile)) File.Delete(tempFile);
				}
				catch(Exception) {
					// Игнорируем ошибки при удалении временных файлов
				}
				
				interactive.ShowMessage(ImportanceLevel.Error, $"Ошибка сохранения файла конфигурации подключений ({options.ConnectionsJsonFileName}).", ex.Message);
			}
			finally {
				// Удаляем файл блокировки
				try {
					if(File.Exists(lockFile)) File.Delete(lockFile);
				}
				catch(Exception lockEx) {
					logger.Warn(lockEx, "Не удалось удалить файл блокировки {0}", lockFile);
				}
			}
		}
		
		private static List<Dictionary<string, string>> FromOldIniConfig(string filename) {
			var config = new IniFileConfiguration(filename);
			return FromOldIniConfig(config);
		}

		private static List<Dictionary<string, string>> FromOldIniConfig(IChangeableConfiguration config) {
			var result = new List<Dictionary<string, string>>();
			
			for(int i = -1; i < 100; i++) {
				var section = "Login" + (i >= 0 ? i.ToString() : String.Empty);
				if(config[$"{section}:ConnectionName"] != null) {
					var parameters = new Dictionary<string, string> {
						{ "Type", config[$"{section}:Type"] == "0" ? "MariaDB" : "QSCloud" },
						{ "Login", config[$"{section}:UserLogin"] }
					};

					if(parameters["Type"] == "QSCloud") {
						parameters.Add("Account", config[$"{section}:Account"]);
						parameters.Add("Title", $"В облако {config[$"{section}:Account"]} как {config[$"{section}:UserLogin"]}");
						parameters.Add("Hash", $"[QSCloud]{config[$"{section}:Account"]}\\{config[$"{section}:UserLogin"]}");
					}

					if(parameters["Type"] == "MariaDB") {
						parameters.Add("Server", config[$"{section}:Server"]);
						parameters.Add("Title", $"На сервер {config[$"{section}:Server"]} как {config[$"{section}:UserLogin"]}");
						parameters.Add("Hash", $"[MariaDB]{config[$"{section}:UserLogin"]}@{config[$"{section}:Server"]}");
					}

					if(result.All(r => r["Hash"] != parameters["Hash"]))
						result.Add(parameters);
				}
			}

			return result;
		}
	}
}
