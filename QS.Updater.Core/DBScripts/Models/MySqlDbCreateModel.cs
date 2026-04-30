using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DBScripts.Models
{
	public class MySqlDbCreateModel : IDBCreator
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly string connectionString;
		private readonly IDbScriptsConfiguration scripts;
		private readonly IProgressBarDisplayable progress;
		private readonly IDbCreatorInteraction interaction;
		private readonly CancellationToken cancellationToken;

		public bool FillBaseGuid { get; set; } = true;

		public MySqlDbCreateModel(
			string connectionString,
			IDbScriptsConfiguration scripts,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Connection string is required", nameof(connectionString));
			this.connectionString = connectionString;
			this.scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}

		public Task<bool> RunCreationAsync(string dbName, string dbTitle) {
			// Тяжёлая часть с MySqlScript.Execute синхронная,
			// поэтому уносим её на пул, чтобы не блокировать UI-поток
			return Task.Run(() => RunCreation(dbName, dbTitle), cancellationToken);
		}

		public bool RunCreation(string dbName, string dbTitle) {
			using(var connectionDB = new MySqlConnection(connectionString)) {
				try {
					logger.Info("Connecting to MySQL...");
					connectionDB.Open();
					cancellationToken.ThrowIfCancellationRequested();

					logger.Info("Проверяем существует ли уже база.");
					var cmd = new MySqlCommand("SHOW DATABASES;", connectionDB);
					bool needDropBase = false;
					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read()) 
						{
							if (rdr[0].ToString() == dbName)
							{
								if (interaction.AskDropExistingDatabaseAsync(dbName).GetAwaiter().GetResult()) 
								{
									needDropBase = true;
									break;
								}
								return false;
							}
						}
					}
					cancellationToken.ThrowIfCancellationRequested();

					logger.Info("Создаем новую базу.");
					progress.Start(text: "Получаем скрипт создания базы");

					string sqlScript = scripts.GetCreationSqlScript();
					int predictedCount = Regex.Matches(sqlScript, ";").Count;

					logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);
					progress.Start(maxValue: predictedCount + (needDropBase ? 2 : 1));

					if (needDropBase) 
					{
						logger.Info("Удаляем существующую базу {0}.", dbName);
						progress.Add(text: $"Удаляем существующую базу {dbName}");
						cmd.CommandText = $"DROP DATABASE `{dbName}`";
						cmd.ExecuteNonQuery();
					}
					cancellationToken.ThrowIfCancellationRequested();

					progress.Add(text: $"Создаем базу {dbName}");
					cmd.CommandText = $"CREATE SCHEMA `{dbName}` DEFAULT CHARACTER SET utf8mb4 ;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = $"USE `{dbName}` ;";
					cmd.ExecuteNonQuery();

					progress.Add(text: $"Создаем таблицы в {dbName}");

					var myscript = new MySqlScript(connectionDB, sqlScript);
					myscript.StatementExecuted += Myscript_StatementExecuted;
					var commands = myscript.Execute();
					logger.Debug("Выполнено {0} SQL-команд.", commands);
					cancellationToken.ThrowIfCancellationRequested();

					// Записываем человекочитаемое название базы, используется для отображения в списке БД
					logger.Info("Записываем Title='{0}' в base_parameters.", dbTitle);
					cmd.CommandText =
						"INSERT INTO base_parameters (name, str_value) VALUES ('Title', @title) "
						+ "ON DUPLICATE KEY UPDATE str_value = @title";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@title", dbTitle ?? string.Empty);
					cmd.ExecuteNonQuery();

					// Версия пустой базы для апдейтера.
					if(scripts.CreationVersion != null) {
						logger.Info("Записываем version='{0}' в base_parameters.", scripts.CreationVersion);
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('version', @ver) "
							+ "ON DUPLICATE KEY UPDATE str_value = @ver";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@ver", scripts.CreationVersion.ToString());
						cmd.ExecuteNonQuery();
					}

					if(FillBaseGuid) {
						logger.Info("Генерируем BaseGuid");
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('BaseGuid', @guid)";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@guid", Guid.NewGuid().ToString());
						cmd.ExecuteNonQuery();
						logger.Info("BaseGuid успешно записан.");
					}
				}
				catch(OperationCanceledException) {
					logger.Info("Создание базы отменено пользователем.");
					throw;
				}
				catch(InvalidCastException ex) { //FIXME Временный для более адекватного обхода проблемы с отсутствием поддержки MariaDB 10.10. Удалить как починим работу с этой версией.
					logger.Error(ex, "Ошибка подключения к серверу.");
					interaction.ReportErrorAsync("Работа с MariaDB 10.10 пока не поддерживается. Установите версию MariaDB 10.9.", lastExecutedStatement)
						.GetAwaiter().GetResult();
					return false;
				}
				catch(MySqlException ex) {
					logger.Error(ex, "Ошибка работы с MySQL.");
					string text;
					if(ex.Number == 1045 || ex.Number == 0)
						text = "Доступ запрещен.\nПроверьте логин и пароль.";
					else if(ex.Number == 1042)
						text = "Не удалось подключиться к серверу БД.";
					else
						text = ex.Message;
					interaction.ReportErrorAsync(text, lastExecutedStatement).GetAwaiter().GetResult();
					return false;
				}
				finally {
					if(progress.IsStarted)
						progress.Close();
				}
			}
			return true;
		}

		private string lastExecutedStatement;
		private void Myscript_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			progress.Add();
			logger.Debug("SQL Command = {0}", args.StatementText);
			lastExecutedStatement = $"[{args.Line}:{args.Position}]{args.StatementText}";
		}
	}
}
