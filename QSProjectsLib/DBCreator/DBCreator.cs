using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using MySql.Data.MySqlClient;

namespace QSProjectsLib
{
	public static class DBCreator
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static readonly List<CreationScript> Scripts = new List<CreationScript>();

		private static CreatorProgress progressDlg;

		public static void AddBaseScript(Version version, string name, string scriptResource)
		{
			Scripts.Add(new CreationScript
			{
				Version = version,
				Name = name,
				Resource = scriptResource
			});
		}

		public static void RunCreation(string server, string dbname)
		{
			string login, password;

			var passwordDlg = new GetPassword ("root");
			passwordDlg.Show ();
			if (passwordDlg.Run () == (int)ResponseType.Ok)
			{
				login = passwordDlg.Login;
				password = passwordDlg.Password;
				passwordDlg.Destroy ();
				RunCreation (null, server, dbname, login, password);
			}
			else
			{
				passwordDlg.Destroy ();
			}
		}

		public static void RunCreation(CreationScript script, string server, string dbname, string login, string password)
		{
			if (script == null)
				script = Scripts.First ();

			string connStr, host;
			uint port = 3306;
			string[] uriSplit = server.Split (new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

			host = uriSplit [0];
			if (uriSplit.Length > 1) {
				uint.TryParse(uriSplit [1], out port);
			}

			var conStrBuilder = new MySqlConnectionStringBuilder();
			conStrBuilder.Server = host;
			conStrBuilder.Port = port;
			conStrBuilder.UserID = login;
			conStrBuilder.Password = password;

			connStr = conStrBuilder.GetConnectionString(true);

			var connectionDB = new MySqlConnection (connStr);
			try {
				logger.Info ("Connecting to MySQL...");

				connectionDB.Open();

			} catch (MySqlException ex) {
				logger.Info ("Строка соединения: {0}", connStr);
				logger.Error (ex, "Ошибка подключения к серверу.");
				if (ex.Number == 1045 || ex.Number == 0)
					MessageDialogWorks.RunErrorDialog ("Доступ запрещен.\nПроверьте логин и пароль.");
				else if (ex.Number == 1042)
					MessageDialogWorks.RunErrorDialog ("Не удалось подключиться к серверу БД.");
				else
					MessageDialogWorks.RunErrorDialog ("Ошибка соединения с базой данных.");

				connectionDB.Close ();
				return;
			}

			logger.Info ("Проверяем существует ли уже база.");

			var sql = "SHOW DATABASES;";
			var cmd = new MySqlCommand (sql, connectionDB);
			bool needDropBase = false;
			using (var rdr = cmd.ExecuteReader ()) 
			{
				while (rdr.Read ()) {
					if (rdr [0].ToString () == dbname) {
						if (MessageDialogWorks.RunQuestionDialog ("База с именем `{0}` уже существует на сервере. Удалить существующую базу перед соданием новой?", dbname)) {
							needDropBase = true;
							break;
						} else
							return;
					}
				}
			}

			logger.Info ("Создаем новую базу.");
			progressDlg = new CreatorProgress ();
			progressDlg.OperationText = "Получаем скрипт создания базы";
			progressDlg.Show ();

			string sqlScript;
			using(Stream stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(script.Resource))
			{
				if(stream == null)
					throw new InvalidOperationException( String.Format("Ресурс {0} со скриптом не найден.", script.Resource));
				StreamReader reader = new StreamReader(stream);
				sqlScript = reader.ReadToEnd();
			}

			int predictedCount = Regex.Matches(sqlScript, ";").Count;

			logger.Debug ("Предполагаем наличие {0} команд в скрипте.", predictedCount);
			progressDlg.OperationText = String.Format ("Создаем базу <{0}>", dbname);
			progressDlg.OperationPartCount = predictedCount + (needDropBase ? 2 : 1);
			progressDlg.OperationCurPart = 0;

			if(needDropBase)
			{
				logger.Info ("Удаляем существующую базу {0}.", dbname);
				progressDlg.OperationText = String.Format ("Удаляем существующую базу {0}", dbname);
				cmd.CommandText = String.Format ("DROP DATABASE `{0}`", dbname);
				cmd.ExecuteNonQuery ();
				progressDlg.OperationCurPart++;
			}

			cmd.CommandText = String.Format ("CREATE SCHEMA `{0}` DEFAULT CHARACTER SET utf8 ;", dbname);
			cmd.ExecuteNonQuery ();
			cmd.CommandText = String.Format ("USE `{0}` ;", dbname);
			cmd.ExecuteNonQuery ();

			progressDlg.OperationText = String.Format ("Создаем таблицы в <{0}>", dbname);
			progressDlg.OperationCurPart++;

			var myscript = new MySqlScript(connectionDB, sqlScript);
			myscript.StatementExecuted += Myscript_StatementExecuted;;
			var commands = myscript.Execute ();
			logger.Debug ("Выполнено {0} SQL-команд.", commands);

			progressDlg.Destroy ();
			progressDlg = null;

			MessageDialogWorks.RunInfoDialog ("Создание базы успешно завершено.\nЗайдите в программу под администратором для добавления пользователей.");
		}

		static void Myscript_StatementExecuted (object sender, MySqlScriptEventArgs args)
		{
			progressDlg.OperationCurPart ++;
			logger.Debug ("SQL Command = {0}", args.StatementText);
		}
	}

	public class CreationScript
	{
		public string Name;
		public Version Version;

		public String Resource;
	}
}

