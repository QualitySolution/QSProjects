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
			try {
				using(var stream = File.Open(options.ConnectionsJsonFileName, FileMode.Open)) {
					try {
						if(stream.Length == 0)
							throw new FileNotFoundException("Файл пуст.");
						connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
					}
					catch(JsonException jsonEx) {
						logger.Error(jsonEx, "Ошибка чтения файла конфигурации подключений ({0}).", options.ConnectionsJsonFileName);
						interactive.ShowMessage(ImportanceLevel.Error, $"Файл конфигурация подключений ({options.ConnectionsJsonFileName}) испорчен. Подключения на будут загружены.");
					}
				}
			}
			catch(IOException exception) when(exception is FileNotFoundException || exception is DirectoryNotFoundException) {
				if(!string.IsNullOrEmpty(options.OldConfigFilename)) {
					if(File.Exists(options.OldConfigFilename)) {
						connectionDefinitions = FromOldIniConfig(options.OldConfigFilename);
						logger.Info("Найден файл с подключениями старой версии, будет создан новый на его основе.");
						interactive.ShowMessage(ImportanceLevel.Info,"Найден файл с подключениями старой версии, будет создан новый на его основе.", "Конфигурация обновлена");
					}
				}
				else {
					connectionDefinitions = options.MakeDefaultConnections?.Invoke();
				}
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
			try {
				// Убедимся, что директория существует
				string directory = Path.GetDirectoryName(options.ConnectionsJsonFileName);
				if (!Directory.Exists(directory)) {
					Directory.CreateDirectory(directory);
				}
				logger.Debug("Перезаписываем файл с соединениями");
				using(var stream = File.Open(options.ConnectionsJsonFileName, FileMode.Create)) {
					JsonSerializer.SerializeAsync(stream, connectionDefinitions, serializerOptions);
				}
				logger.Debug("Записали соединения соединениями");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка сохранения файла конфигурации подключений ({0}).", options.ConnectionsJsonFileName);
				interactive.ShowMessage(ImportanceLevel.Error, $"Ошибка сохранения файла конфигурации подключений ({options.ConnectionsJsonFileName}).", ex.Message);
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
