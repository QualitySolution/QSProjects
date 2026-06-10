using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QS.DBScripts.Models
{
	public class MySqlDbCreateModel : IDbCreatorModel
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly string connectionString;
		private readonly CreationScript scripts;
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
			this.scripts = scripts.MakeCreationScript() ?? throw new ArgumentNullException(nameof(scripts));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}

		public MySqlDbCreateModel(
			string server, uint port, string login, string password,
			IDbScriptsConfiguration scripts,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken) {

			this.connectionString = new MySqlConnectionStringBuilder {
				Server = server,
				Port = port,
				UserID = login,
				Password = password,
				AllowUserVariables = true
			}.ConnectionString;
			this.scripts = scripts.MakeCreationScript() ?? throw new ArgumentNullException(nameof(scripts));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}


		public bool RunCreation(string dbName, string dbTitle = null) {
			using(var connectionDB = new MySqlConnection(connectionString)) {
				try {
					logger.Info("Connecting to MySQL...");
					connectionDB.Open();

					logger.Info("Проверяем существует ли уже база.");

					var sql = "SHOW DATABASES;";
					var cmd = new MySqlCommand(sql, connectionDB);
					bool needDropBase = false;
					bool hasBase = false;
					using(var rdr = cmd.ExecuteReader()) {
						while(rdr.Read()) {
							if(rdr[0].ToString() == dbName) {
								if(interaction.AskDropExistingDatabase(dbName)) {
									needDropBase = true;
								}
								hasBase = true;
								break;
							}
						}
					}

					logger.Info("Создаем новую базу.");

					progress.Start(text: "Получаем скрипт создания базы");

					string sqlScript = scripts.GetSqlScript();
					int predictedCount = Regex.Matches(sqlScript, ";").Count;

					logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);
					progress.Start(maxValue: predictedCount + (needDropBase ? 2 : 1));

					if(needDropBase) {
						logger.Info("Удаляем существующую базу {0}.", dbName);
						progress.Add(text: $"Удаляем существующую базу {dbName}");
						cmd.CommandText = String.Format("DROP DATABASE `{0}`", dbName);
						cmd.ExecuteNonQuery();
					}

					if(!hasBase || needDropBase) {
						progress.Add(text: $"Создаем базу {dbName}");
						cmd.CommandText = String.Format("CREATE SCHEMA `{0}` DEFAULT CHARACTER SET utf8mb4 ;", dbName);
						cmd.ExecuteNonQuery();
					}
					cmd.CommandText = String.Format("USE `{0}` ;", dbName);
					cmd.ExecuteNonQuery();

					progress.Add(text: $"Создаем таблицы в {dbName}");

					var myscript = new MySqlScript(connectionDB, sqlScript);
					myscript.StatementExecuted += Myscript_StatementExecuted;
					var commands = myscript.Execute();
					logger.Debug("Выполнено {0} SQL-команд.", commands);

					if(FillBaseGuid) {
						logger.Info("Генерируем BaseGuid");
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('BaseGuid', @guid)";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@guid", Guid.NewGuid().ToString());
						cmd.ExecuteNonQuery();
						logger.Info("BaseGuid успешно записан.");
					}

					if(dbTitle != null) {
						logger.Info("Генерируем BaseTitle");
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('BaseTitle', @title)";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@title", dbTitle);
						cmd.ExecuteNonQuery();
						logger.Info("BaseTitle успешно записан.");
					}

				}
				catch(OperationCanceledException) {
					logger.Info("Создание базы отменено пользователем.");
					throw;
				}
				catch(InvalidCastException ex) { //FIXME Временный для более адекватного обхода проблемы с отсутствием поддержки MariaDB 10.10. Удалить как починим работу с этой версией.
					logger.Error(ex, "Ошибка подключения к серверу.");
					interaction.ReportError("Работа с MariaDB 10.10 пока не поддерживается. Установите версию MariaDB 10.9.", lastExecutedStatement);
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
					interaction.ReportError(text, lastExecutedStatement);
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
