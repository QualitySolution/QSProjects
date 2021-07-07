using System;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using QS.DBScripts.Controllers;

namespace QS.DBScripts.Models
{
	public class MySqlDbCreateModel
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IDbCreateController controller;
		private readonly CreationScript script;

		public MySqlDbCreateModel(IDbCreateController controller, CreationScript script)
		{
			this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
			this.script = script ?? throw new ArgumentNullException(nameof(script));
		}

		public bool RunCreation(string server, string dbname, string login, string password)
		{
			string connStr, host;
			uint port = 3306;
			string[] uriSplit = server.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

			host = uriSplit[0];
			if (uriSplit.Length > 1) {
				uint.TryParse(uriSplit[1], out port);
			}

			var conStrBuilder = new MySqlConnectionStringBuilder();
			conStrBuilder.Server = host;
			conStrBuilder.Port = port;
			conStrBuilder.UserID = login;
			conStrBuilder.Password = password;

			connStr = conStrBuilder.GetConnectionString(true);

			using (var connectionDB = new MySqlConnection(connStr)) {
				try {
					logger.Info("Connecting to MySQL...");
					connectionDB.Open();

				}
				catch (MySqlException ex) {
					logger.Info("Строка соединения: {0}", connStr);
					logger.Error(ex, "Ошибка подключения к серверу.");
					if (ex.Number == 1045 || ex.Number == 0)
						controller.WasError("Доступ запрещен.\nПроверьте логин и пароль.");
					else if (ex.Number == 1042)
						controller.WasError("Не удалось подключиться к серверу БД.");
					else
						controller.WasError("Ошибка соединения с базой данных.");
					
					return false;
				}

				logger.Info("Проверяем существует ли уже база.");

				var sql = "SHOW DATABASES;";
				var cmd = new MySqlCommand(sql, connectionDB);
				bool needDropBase = false;
				using (var rdr = cmd.ExecuteReader()) {
					while (rdr.Read()) {
						if (rdr[0].ToString() == dbname) {
							if (controller.BaseExistDropIt(dbname)) {
								needDropBase = true;
								break;
							}
							else
								return false;
						}
					}
				}

				logger.Info("Создаем новую базу.");

				controller.Progress.Start(text: "Получаем скрипт создания базы");

				string sqlScript = script.GetSqlScript();
				int predictedCount = Regex.Matches(sqlScript, ";").Count;

				logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);
				controller.Progress.Start(maxValue: predictedCount + (needDropBase ? 2 : 1));

				if (needDropBase) {
					logger.Info("Удаляем существующую базу {0}.", dbname);
					controller.Progress.Add(text: $"Удаляем существующую базу {dbname}");
					cmd.CommandText = String.Format("DROP DATABASE `{0}`", dbname);
					cmd.ExecuteNonQuery();
				}

				controller.Progress.Add(text: $"Создаем базу <{dbname}>");
				cmd.CommandText = String.Format("CREATE SCHEMA `{0}` DEFAULT CHARACTER SET utf8mb4 ;", dbname);
				cmd.ExecuteNonQuery();
				cmd.CommandText = String.Format("USE `{0}` ;", dbname);
				cmd.ExecuteNonQuery();

				controller.Progress.Add(text: $"Создаем таблицы в <{dbname}>");

				var myscript = new MySqlScript(connectionDB, sqlScript);
				myscript.StatementExecuted += Myscript_StatementExecuted;
				var commands = myscript.Execute();
				logger.Debug("Выполнено {0} SQL-команд.", commands);
			}

			controller.Progress.Close();
			return true;
		}

		void Myscript_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			controller.Progress.Add();
			logger.Debug("SQL Command = {0}", args.StatementText);
		}
	}
}
