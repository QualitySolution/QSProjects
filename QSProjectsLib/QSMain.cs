using System;
using System.Reflection;
using System.Data.Common;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using NLog;

namespace QSProjectsLib
{
	public static class QSMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static DataProviders DBMS;
		public static DbProviderFactory ProviderDB;
		public static MySqlConnection connectionDB;
		public static string ConnectionString;
		public static Dictionary<string, UserPermission> ProjectPermission;
		public static Dictionary<string, TableInfo> ProjectTables;
		public static string AdminFieldName;
		public static UserInfo User;
		//События
		[Obsolete("Не вызывается в новой версии, используейте NLog, для получения сообщений.")]
		public static event EventHandler<NewStatusTextEventArgs> NewStatusText;
		public static event EventHandler<ReferenceUpdatedEventArgs> ReferenceUpdated;

		//Перечисления
		public enum DataProviders {MySQL, Factory};

		private static DbConnection _ConnectionDB;
		public static DbConnection ConnectionDB
		{
			get
			{
				switch (DBMS) 
				{
					case DataProviders.Factory:
						return _ConnectionDB;
					case DataProviders.MySQL:
					default:
						return connectionDB;
				}
			}
			set
			{
				switch (DBMS) 
				{
					case DataProviders.Factory:
						_ConnectionDB = value;
						break;
					case DataProviders.MySQL:
					default:
						connectionDB = (MySqlConnection) value;
						break;
				}
			}
		}

		//Событие строки состояния
		[Obsolete("Не вызывается в новой версии, используейте NLog, для получения сообщений.")]
		public class NewStatusTextEventArgs : EventArgs
		{
			public string NewText { get; set; }
		}

		//Регистрируем правила Nlog для строки состояния
		public static void MakeNewStatusTargetForNlog(string methodName, string className)
		{
			NLog.Config.LoggingConfiguration config = LogManager.Configuration;
			if (config.FindTargetByName("status") != null)
				return;
			NLog.Targets.MethodCallTarget targetLog = new NLog.Targets.MethodCallTarget();
			targetLog.Name = "status";
			targetLog.MethodName = methodName;
			targetLog.ClassName = className;
			targetLog.Parameters.Add(new NLog.Targets.MethodCallParameter( "${message}"));
			config.AddTarget("status", targetLog);
			NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule("*", LogLevel.Info, targetLog);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;
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
				logger.Warn("Не передано соединение с БД!");
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
			if (DBMS != DataProviders.MySQL)
				return;
			if(!connectionDB.Ping())
			{
				logger.Warn("Соединение с сервером разорвано, пробуем пересоединится...");
				logger.Info("Пытаемся восстановить соединение...");
				try
				{
					connectionDB.Open();
				}
				catch (Exception ex) 
				{
					logger.FatalException("Пересоединится не удалось.", ex);
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

		public static void ErrorMessage(Window parent, Exception ex, string userMessage = "")
		{
			QSSupportLib.ErrorMsg md = new QSSupportLib.ErrorMsg(parent, ex, userMessage);
			md.Run ();
			md.Destroy();
		}

		public static void RunAboutDialog()
		{
			AboutDialog dialog = new AboutDialog ();

			System.Reflection.Assembly assembly = Assembly.GetCallingAssembly();
			object[] att = assembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);

			dialog.ProgramName = ((AssemblyTitleAttribute)att [0]).Title;;

			Version version = assembly.GetName().Version;
			dialog.Version = version.ToString (version.Revision == 0 ? (version.Build == 0 ? 2 : 3) : 4);

			att = assembly.GetCustomAttributes (typeof(AssemblyLogoIcon), false);
			if (att.Length > 0)
			{
				dialog.Logo = new Gdk.Pixbuf(assembly, ((AssemblyLogoIcon)att[0]).ResourceName); //Gdk.Pixbuf.LoadFromResource();
			}

			att = assembly.GetCustomAttributes (typeof(AssemblyDescriptionAttribute), false);

			string comments = ((AssemblyDescriptionAttribute)att[0]).Description;

			att = assembly.GetCustomAttributes (typeof(AssemblySupport), false);
			if(att.Length > 0)
			{
				AssemblySupport sup = (AssemblySupport)att[0];
				if (sup.ShowTechnologyUsed)
					comments += String.Format("\nРазработана на MonoDevelop с использованием открытых технологий Mono, GTK#, Nlog{0}.", sup.TechnologyUsed != "" ? ", " + sup.TechnologyUsed : "");
				if(sup.SupportInfo != "")
					comments += sup.SupportInfo;
				else 
					comments += "\nТелефон тех. поддержки +7(812)575-79-44";
			}
			dialog.Comments = comments;

			att = assembly.GetCustomAttributes (typeof(AssemblyCopyrightAttribute), false);

			dialog.Copyright = ((AssemblyCopyrightAttribute)att[0]).Copyright;

			att = assembly.GetCustomAttributes (typeof(AssemblyAuthor), false);

			List<string> authors = new List<string>();
			foreach(AssemblyAuthor author in att)
			{
				authors.Add(author.Name);
			}
			authors.Reverse();
			dialog.Authors = authors.ToArray();

			att = assembly.GetCustomAttributes (typeof(AssemblyAppWebsite), false);

			if(att.Length > 0)
				dialog.Website = ((AssemblyAppWebsite)att[0]).Link;

			dialog.Run ();
			dialog.Destroy();
		}

		public static void WaitRedraw()
		{
			while (Application.EventsPending ())
			{
				Gtk.Main.Iteration ();
			}
		}

	}
}

