using System;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using NLog;

namespace QSProjectsLib
{
	public class QSMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static MySqlConnection connectionDB;
		public static string ConnectionString;
		public static Dictionary<string, UserPermission> ProjectPermission;
		public static Dictionary<string, TableInfo> ProjectTables;
		public static string AdminFieldName;
		public static UserInfo User;
		//События
		public static event EventHandler<NewStatusTextEventArgs> NewStatusText;
		public static event EventHandler<ReferenceUpdatedEventArgs> ReferenceUpdated;

		public QSMain ()
		{
		}

		//Событие строки состояния
		public class NewStatusTextEventArgs : EventArgs
		{
			public string NewText { get; set; }
		}

		internal static void OnNewStatusText(string NewText)
		{
			EventHandler<NewStatusTextEventArgs> handler = NewStatusText;
			if (handler != null)
			{
				NewStatusTextEventArgs e = new NewStatusTextEventArgs();
				e.NewText = NewText;
				handler(null, e);
			}
		}

		//Событие обновления справочников
		public class ReferenceUpdatedEventArgs : EventArgs
		{
			public string ReferenceTable { get; set; }
		}
		
		internal static void OnReferenceUpdated(string Table)
		{
			EventHandler<ReferenceUpdatedEventArgs> handler = ReferenceUpdated;
			if (handler != null)
			{
				ReferenceUpdatedEventArgs e = new ReferenceUpdatedEventArgs();
				e.ReferenceTable = Table;
				handler(null, e);
			}
		}

		internal static bool TestConnection()
		{
			if(connectionDB == null)
			{
				QSMain.OnNewStatusText("Не передано соединение с БД!");
				return false;
			}
			return true;
		}

		public static void CheckServer(Window Parrent)
		{
			string sql = "SHOW VARIABLES LIKE \"character_set_%\";";
			MySqlCommand cmd = new MySqlCommand (sql, connectionDB);
			string TextMes = "";
			using (MySqlDataReader rdr = cmd.ExecuteReader ()) 
			{
				while (rdr.Read ()) 
				{
					switch (rdr ["Variable_name"].ToString ()) 
					{
					case "character_set_server":
						if (rdr ["Value"].ToString () != "utf8") {
							TextMes += String.Format ("* character_set_server = {0} - для нормальной работы программы кодировка сервера " +
								"должна быть utf8, иначе возможны проблемы с языковыми символами, этот параметр изменяется" +
							"в настройках сервера.\n", rdr ["Value"].ToString ());
						}
						break;
					case "character_set_database":
						if (rdr ["Value"].ToString () != "utf8") {
							TextMes += String.Format ("* character_set_database = {0} - для нормальной работы программы кодировка базы данных " +
								"должна быть utf8, иначе возможны проблемы с языковыми символами, измените кодировку для используемой базы.\n", rdr ["Value"].ToString ());
						}
						break;
					}
				}
			}
			if (TextMes != "") 
			{
				MessageDialog VersionError = new MessageDialog (Parrent, DialogFlags.DestroyWithParent,
					                            MessageType.Warning, 
					                            ButtonsType.Close, 
					                            TextMes);
				VersionError.Run ();
				VersionError.Destroy ();
			}
		}

		public static void CheckConnectionAlive()
		{
			if(!connectionDB.Ping())
			{
				logger.Warn("Соединение с сервером разорвано, пробуем пересоединится...");
				QSMain.OnNewStatusText("Пытаемся восстановить соединение...");
				try
				{
					connectionDB.Open();
				}
				catch (Exception ex) 
				{
					logger.Fatal("Пересоединится не удалось.");
					logger.Fatal(ex.ToString());
					//ErrorMessage(this,ex);
					throw;
				}
			}
		}

		public static string GetPermissionFieldsForSelect()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", " + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForInsert()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForUpdate()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", " + Right.Value.DataBaseName + " = @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static void ErrorMessage(Window parent, Exception ex)
		{
			QSSupportLib.ErrorMsg md = new QSSupportLib.ErrorMsg(parent, ex);
			md.Run ();
			md.Destroy();
		}
	}
}

