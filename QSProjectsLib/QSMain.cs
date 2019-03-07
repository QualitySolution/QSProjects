using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;

namespace QSProjectsLib
{
	public static class QSMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public static Window ErrorDlgParrent;
		public static Thread GuiThread;

		static bool statusBarRedrawHandled;
		private static Label statusBarLabel;
		public static Label StatusBarLabel{ get{ 
				return statusBarLabel;
			}
			set{ 
				if (statusBarLabel != null)
					statusBarLabel.ExposeEvent -= OnStatusBarExposed;
				statusBarLabel = value;
				statusBarLabel.ExposeEvent += OnStatusBarExposed;
			}}
				
		//Работа с базой
		public static DataProviders DBMS;
		public static DbProviderFactory ProviderDB;
		public static MySqlConnection connectionDB;
		public static string ConnectionString;
		public static Dictionary<string, UserPermission> ProjectPermission;
		public static Dictionary<string, TableInfo> ProjectTables;
		public static UserInfo User;

		private static string adminFieldName = "admin";

		public static string AdminFieldName {
			get { return adminFieldName; }
			set {
				adminFieldName = value;
				logger.Warn ("В установке полю AdminFieldName значения admin нет необходимости, оно имеет его по умолчанию. " +
				"Другое название колонки в базе приведет к некорректной работе c SAAS.");
			}
		}

		//События
		public static event EventHandler<ReferenceUpdatedEventArgs> ReferenceUpdated;
		public static event EventHandler<RunErrorMessageDlgEventArgs> RunErrorMessageDlg;

		/// <summary>
		/// Используется для интеграции QSProjectsLib и новым механизмом удаления в QSOrmProject,
		/// для других целей использовать не желательно.
		/// </summary>
		public static event EventHandler<RunOrmDeletionEventArgs> RunOrmDeletion;

		//Перечисления
		public enum DataProviders
		{
			MySQL,
			Factory
		};

		//Внутриннии
		internal static bool WaitResultIsOk;

		private static DbConnection _ConnectionDB;

		public static DbConnection ConnectionDB {
			get {
				switch (DBMS) {
					case DataProviders.Factory:
						return _ConnectionDB;
					case DataProviders.MySQL:
						if((connectionDB == null || (connectionDB != null && connectionDB.State == System.Data.ConnectionState.Closed)) 
							&& !String.IsNullOrWhiteSpace(QSMain.ConnectionString))
						{
							connectionDB = new MySqlConnection (QSMain.ConnectionString);
							connectionDB.Open ();
						}
						return connectionDB;
					default:
						return connectionDB;
				}
			}
			set {
				switch (DBMS) {
				case DataProviders.Factory:
					_ConnectionDB = value;
					break;
				case DataProviders.MySQL:
				default:
					connectionDB = (MySqlConnection)value;
					break;
				}
			}
		}

		public class RunErrorMessageDlgEventArgs : EventArgs
		{
			public Window ParentWindow { get; set; }

			public Exception Exception { get; set; }

			public string UserMessage { get; set; }

			public RunErrorMessageDlgEventArgs (Window parent, Exception ex, string userMessage)
			{
				ParentWindow = parent;
				Exception = ex;
				UserMessage = userMessage;
			}
		}

		static void OnStatusBarExposed(object sender, EventArgs args){
			statusBarRedrawHandled = true;
		}

		/// <summary>
		/// Регистрируем правила Nlog для строки состояния
		/// </summary>
		/// <param name="methodName">Имя статического метода который будет вызываться при появлении сообщения.</param>
		/// <param name="className">Имя класа в котором находится метод.</param>
		public static void MakeNewStatusTargetForNlog (string methodName, string className)
		{
			NLog.Config.LoggingConfiguration config = LogManager.Configuration;
			if (config.FindTargetByName ("status") != null)
				return;
			NLog.Targets.MethodCallTarget targetLog = new NLog.Targets.MethodCallTarget ();
			targetLog.Name = "status";
			targetLog.MethodName = methodName;
			targetLog.ClassName = className;
			targetLog.Parameters.Add (new NLog.Targets.MethodCallParameter ("${message}"));
			config.AddTarget ("status", targetLog);
			NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule ("*", targetLog);
			rule.EnableLoggingForLevel (LogLevel.Info);
			config.LoggingRules.Add (rule);

			LogManager.Configuration = config;
		}

		/// <summary>
		/// Регистрируем правила Nlog для строки состояния
		/// </summary>
		public static void MakeNewStatusTargetForNlog ()
		{
			MakeNewStatusTargetForNlog ("StatusMessage", "QSProjectsLib.QSMain, QSProjectsLib");
		}

		//Событие обновления справочников
		public class ReferenceUpdatedEventArgs : EventArgs
		{
			public string ReferenceTable { get; set; }
		}

		internal static void OnReferenceUpdated (string Table)
		{
			EventHandler<ReferenceUpdatedEventArgs> handler = ReferenceUpdated;
			if (handler != null) {
				ReferenceUpdatedEventArgs e = new ReferenceUpdatedEventArgs ();
				e.ReferenceTable = Table;
				handler (null, e);
			}
		}

		public class RunOrmDeletionEventArgs : EventArgs
		{
			public string TableName { get; set; }

			public int ObjectId { get; set; }

			public bool Result { get; set; }
		}

		public static bool IsOrmDeletionConfigered {
			get { return RunOrmDeletion != null; }
		}

		internal static bool OnOrmDeletion (string table, int id)
		{
			EventHandler<RunOrmDeletionEventArgs> handler = RunOrmDeletion;
			if (handler != null) {
				var e = new RunOrmDeletionEventArgs {
					TableName = table,
					ObjectId = id
				};
				handler (null, e);
				return e.Result;
			}
			return false;
		}


		internal static bool TestConnection ()
		{
			if (connectionDB == null) {
				logger.Warn ("Не передано соединение с БД!");
				return false;
			}
			return true;
		}

		public static void CheckServer ()
		{
			CheckServer (null);
		}

		/// <summary>
		/// Проверка локали сервера.
		/// </summary>
		/// <param name="parent">Если Parent = null, сообщение будет выводиться в nlog. В противном случае в диалоговое окно.</param>
		public static void CheckServer (Window parent)
		{
			string sql = "SHOW VARIABLES LIKE \"character_set_%\";";
			MySqlCommand cmd = new MySqlCommand (sql, connectionDB);
			string TextMes = "";
			logger.Debug (sql);
			using (MySqlDataReader rdr = cmd.ExecuteReader ()) {
				while (rdr.Read ()) {
					logger.Debug (String.Format ("{0} = {1}", rdr ["Variable_name"], rdr ["Value"]));
					switch (rdr ["Variable_name"].ToString ()) {
					case "character_set_server":
						if (rdr ["Value"].ToString () != "utf8" && rdr["Value"].ToString() != "utf8mb4") {
							TextMes += String.Format ("* character_set_server = {0} - для нормальной работы программы кодировка сервера " +
							"должна быть utf8 или utf8mb4, иначе возможны проблемы с языковыми символами, этот параметр изменяется " +
							"в настройках сервера MySQL\\MariaDB.\n", rdr ["Value"].ToString ());
						}
						break;
					case "character_set_database":
						if (rdr ["Value"].ToString () != "utf8" && rdr["Value"].ToString() != "utf8mb4") {
							TextMes += String.Format ("* character_set_database = {0} - для нормальной работы программы кодировка базы данных " +
							"должна быть utf8 или utf8mb4, иначе возможны проблемы с языковыми символами, измените кодировку для используемой базы.\n", rdr ["Value"].ToString ());
						}
						break;
					}
				}
			}
			if (TextMes != "") {
				logger.Warn(TextMes);
				MessageDialog VersionError = new MessageDialog (parent, DialogFlags.DestroyWithParent,
					                             MessageType.Warning, 
					                             ButtonsType.Close, 
					                             TextMes);
				VersionError.Run ();
				VersionError.Destroy ();
			}
		}

		public static void DoPing ()
		{
			connectionDB.Ping ();
			logger.Debug ("Конец пинга соединения.");
		}

		public static void DoConnect ()
		{
			WaitResultIsOk = true;
			try {
				connectionDB.Open ();
			} catch (Exception ex) {
				logger.Warn (ex, "Не удалось соединится.");
				WaitResultIsOk = false;
			}
		}

		public static void TryConnect ()
		{
			logger.Info ("Пытаемся восстановить соединение...");
			bool timeout = WaitOperationDlg.RunOperationWithDlg (new ThreadStart (DoConnect),
				               connectionDB.ConnectionTimeout,
				               "Соединяемся с сервером MySQL."
			               );
			if (!WaitResultIsOk || timeout) {
				MessageDialog md = new MessageDialog (null, 
					                   DialogFlags.DestroyWithParent,
					                   MessageType.Question,
					                   ButtonsType.YesNo,
					                   "Соединение было разорвано. Повторить попытку подключения? В противном случае приложение завершит работу.");
				ResponseType result = (ResponseType)md.Run ();
				md.Destroy ();
				if (result == ResponseType.Yes) {
					TryConnect ();
				} else {
					Environment.Exit (1);
				}
			}
		}

		public static void CheckConnectionAlive ()
		{
			if (DBMS != DataProviders.MySQL)
				return;
			logger.Info ("Проверяем соединение...");

			bool timeout = WaitOperationDlg.RunOperationWithDlg (new ThreadStart (DoPing),
				               connectionDB.ConnectionTimeout,
				               "Идет проверка соединения с базой данных.");
			if (timeout && ConnectionDB.State == System.Data.ConnectionState.Open) {
				ConnectionDB.Close (); //На линуксе есть случаи когда состояние соедиения не корректное.
			}
			if (connectionDB.State != System.Data.ConnectionState.Open) {
				logger.Warn ("Соединение с сервером разорвано, пробуем пересоединится...");
				TryConnect ();
			}
			logger.Info ("Ок.");
		}


		public static string GetPermissionFieldsForSelect ()
		{
			if (ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach (KeyValuePair<string, UserPermission> Right in ProjectPermission) {
				FieldsString += ", " + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForInsert ()
		{
			if (ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach (KeyValuePair<string, UserPermission> Right in ProjectPermission) {
				FieldsString += ", @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForUpdate ()
		{
			if (ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach (KeyValuePair<string, UserPermission> Right in ProjectPermission) {
				FieldsString += ", " + Right.Value.DataBaseName + " = @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static void ErrorMessageWithLog (string userMessage, Logger logger, Exception ex, LogLevel level = null)
		{
			if (level == null)
				level = LogLevel.Error;
			logger.Log (level, ex, userMessage);
			ErrorMessage (ex, userMessage);
		}

		public static void ErrorMessageWithLog (Window parent, string userMessage, Logger logger, Exception ex, LogLevel level = null)
		{
			if (level == null)
				level = LogLevel.Error;
			logger.Log (level, ex, userMessage);
			ErrorMessage (parent, ex, userMessage);
		}

		public static void ErrorMessage (Exception ex, string userMessage = "")
		{
			ErrorMessage (ErrorDlgParrent, ex, userMessage);
		}

		public static void ErrorMessage (Window parent, Exception ex, string userMessage = "")
		{
			if (GuiThread == Thread.CurrentThread) {
				RealErrorMessage (parent, ex, userMessage);
			} else {
				logger.Debug ("From Another Thread");
				Application.Invoke (delegate {
					RealErrorMessage (parent, ex, userMessage);
				});
			}
		}

		private static void RealErrorMessage (Window parent, Exception ex, string userMessage = "")
		{
			if (parent == null && ErrorDlgParrent != null)
				parent = ErrorDlgParrent;

			if (RunErrorMessageDlg != null) {
				RunErrorMessageDlg (null, new RunErrorMessageDlgEventArgs (parent, ex, userMessage));
			} else {
				MessageDialog md = new MessageDialog (parent, DialogFlags.DestroyWithParent,
					                   MessageType.Error, 
					                   ButtonsType.Ok, 
					                   userMessage != "" ? userMessage : ex.Message
				                   );
				md.Run ();
				md.Destroy ();
			}
		}

		public static void RunAboutDialog ()
		{
			AboutDialog dialog = new AboutDialog ();

			Assembly assembly = Assembly.GetCallingAssembly ();
			object[] att = assembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
			AssemblyEditionAttribute editionAtt = assembly.GetCustomAttributes (typeof(AssemblyEditionAttribute), false).OfType<AssemblyEditionAttribute> ().FirstOrDefault ();

			dialog.ProgramName = ((AssemblyTitleAttribute)att [0]).Title;

			dialog.Version = StringWorks.VersionToShortString (assembly.GetName ().Version) 
				+ (editionAtt != null && editionAtt.Edition != "gpl" ? String.Format ("({0})", editionAtt.Edition) : String.Empty);

			att = assembly.GetCustomAttributes (typeof(AssemblyLogoIconAttribute), false);
			if (att.Length > 0) {
				dialog.Logo = new Gdk.Pixbuf (assembly, ((AssemblyLogoIconAttribute)att [0]).ResourceName); //Gdk.Pixbuf.LoadFromResource();
			}

			att = assembly.GetCustomAttributes (typeof(AssemblyDescriptionAttribute), false);

			string comments = String.Empty;

			object[] betaAtt = assembly.GetCustomAttributes (typeof(AssemblyBetaBuildAttribute), false);
			if(betaAtt.Length > 0)
			{
				var buildDate = System.IO.File.GetLastWriteTime(assembly.Location);
				comments += String.Format ("Бета редакция от {0:g}\n", buildDate);
			}

			comments += ((AssemblyDescriptionAttribute)att [0]).Description;


			att = assembly.GetCustomAttributes (typeof(AssemblySupportAttribute), false);
			if (att.Length > 0) {
				AssemblySupportAttribute sup = (AssemblySupportAttribute)att [0];
				if (sup.ShowTechnologyUsed)
					comments += String.Format ("\nРазработана на MonoDevelop с использованием открытых технологий Mono, GTK#, Nlog{0}.", sup.TechnologyUsed != "" ? ", " + sup.TechnologyUsed : "");
				if (sup.SupportInfo != "")
					comments += sup.SupportInfo;
				else
					comments += "\nТелефон тех. поддержки +7(812)309-80-89";
			}
			dialog.Comments = comments;

			att = assembly.GetCustomAttributes (typeof(AssemblyCopyrightAttribute), false);

			dialog.Copyright = ((AssemblyCopyrightAttribute)att [0]).Copyright;

			att = assembly.GetCustomAttributes (typeof(AssemblyAuthorAttribute), false);

			List<string> authors = new List<string> ();
			foreach (AssemblyAuthorAttribute author in att) {
				authors.Add (author.Name);
			}
			authors.Reverse ();
			dialog.Authors = authors.ToArray ();

			att = assembly.GetCustomAttributes (typeof(AssemblyAppWebsiteAttribute), false);

			if (att.Length > 0)
				dialog.Website = ((AssemblyAppWebsiteAttribute)att [0]).Link;

			dialog.Run ();
			dialog.Destroy ();
		}

		public static void RunChangeLogDlg (Gtk.Window parent)
		{
			Dialog HistoryDialog = new Dialog ("История версий программы", parent, Gtk.DialogFlags.DestroyWithParent);
			HistoryDialog.Modal = true;
			HistoryDialog.AddButton ("Закрыть", ResponseType.Close);

			System.IO.StreamReader HistoryFile = new System.IO.StreamReader ("changes.txt");
			TextView HistoryTextView = new TextView ();
			HistoryTextView.WidthRequest = 700;
			HistoryTextView.WrapMode = WrapMode.Word;
			HistoryTextView.Sensitive = false;
			HistoryTextView.Buffer.Text = HistoryFile.ReadToEnd ();
			Gtk.ScrolledWindow ScrollW = new ScrolledWindow ();
			ScrollW.HeightRequest = 500;
			ScrollW.Add (HistoryTextView);
			HistoryDialog.VBox.Add (ScrollW);

			HistoryDialog.ShowAll ();
			HistoryDialog.Run ();
			HistoryDialog.Destroy ();
		}

		public static void WaitRedraw ()
		{
			while (Application.EventsPending ()) {
				Gtk.Main.Iteration ();
			}
		}

		static DateTime lastRedraw;
		/// <summary>
		/// Главный цикл приложения будет вызываться не чаще чем время указанное в парамерах.
		/// </summary>
		/// <param name="milliseconds">Milliseconds.</param>
		public static void WaitRedraw(int milliseconds )
		{
			if (DateTime.Now.Subtract(lastRedraw).Milliseconds < milliseconds)
				return;

			lastRedraw = DateTime.Now;
			while (Application.EventsPending())
			{
				Gtk.Main.Iteration();
			}
		}

		public static void StatusMessage (string message)
		{
			if (GuiThread == Thread.CurrentThread) {
				RealStatusMessage (message,true);
			} else {
				Console.WriteLine ("Another Thread");
				Application.Invoke (delegate {
					RealStatusMessage (message,false);
				});
			}
		}

		static void RealStatusMessage (string message, bool waitRedraw)
		{
			if (StatusBarLabel == null)
				return;
			
			StatusBarLabel.LabelProp = message;
			if (!waitRedraw)
				return;		
			statusBarRedrawHandled = false;
			while (Application.EventsPending () && !statusBarRedrawHandled) {
				Gtk.Main.Iteration();
			}
		}

		public static void SubscribeToUnhadledExceptions ()
		{
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				logger.Fatal ((Exception)e.ExceptionObject, "Поймано необработаное исключение в Application Domain.");
				QSMain.ErrorMessage ((Exception)e.ExceptionObject);
			};
			GLib.ExceptionManager.UnhandledException += delegate(GLib.UnhandledExceptionArgs a) {
				logger.Fatal ((Exception)a.ExceptionObject, "Поймано необработаное исключение в GTK.");
				QSMain.ErrorMessage ((Exception)a.ExceptionObject);
			};
		}

		public static void SetupFromArgs(string[] args)
		{
			if(args.Length == 0)
				return;
			for(int i = 0; i < args.Length; i++)
			{
				if(args[i] == "-d" || args[i].ToLower() == "--default")
				{
					if(i + 1 >= args.Length)
					{
						logger.Error("После аргумента {0} должно быть указано название соедиения. А его нет.", args[i]);
						return;
					}

					i++;
					Login.OverwriteDefaultConnection = args[i];
				}
			}
		}
	}
}

