using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Gtk;
using MySqlConnector;
using NLog;
using QS.Utilities.Text;

namespace QSProjectsLib
{
	[Obsolete]
	public static class QSMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		[Obsolete("Настройка оставлена для совместимости со старыми проектами. В новых используйте класс QS.ErrorReporting.GtkUI.UnhandledExceptionHandler")]
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
		public static MySqlConnectionStringBuilder ConnectionStringBuilder;
		public static Dictionary<string, UserPermission> ProjectPermission = new Dictionary<string, UserPermission>();
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

		public static void ChangeConnectionString(string newConnectionString)
		{
			ConnectionString = newConnectionString;
			connectionDB = new MySqlConnection(newConnectionString);
			ConnectionStringBuilder.ConnectionString = newConnectionString;
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

		//Внутренние
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
		/// <param name="className">Имя класса в котором находится метод.</param>
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
				ConnectionDB.Close (); //На линуксе есть случаи когда состояние соединения не корректное.
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

		[Obsolete("Метод использует старый диалог вывода сообщения об ошибке. При переходе на класс UnhandledExceptionHandler, лучше не использовать старые сообщения.")]
		public static void ErrorMessageWithLog (string userMessage, Logger logger, Exception ex, LogLevel level = null)
		{
			if (level == null)
				level = LogLevel.Error;
			logger.Log (level, ex, userMessage);
			ErrorMessage (ex, userMessage);
		}

		[Obsolete("Метод использует старый диалог вывода сообщения об ошибке. При переходе на класс UnhandledExceptionHandler, лучше не использовать старые сообщения.")]
		public static void ErrorMessageWithLog (Window parent, string userMessage, Logger logger, Exception ex, LogLevel level = null)
		{
			if (level == null)
				level = LogLevel.Error;
			logger.Log (level, ex, userMessage);
			ErrorMessage (parent, ex, userMessage);
		}

		[Obsolete("Метод использует старый диалог вывода сообщения об ошибке. При переходе на класс UnhandledExceptionHandler, лучше не использовать старые сообщения.")]
		public static void ErrorMessage (Exception ex, string userMessage = "")
		{
			ErrorMessage (ErrorDlgParrent, ex, userMessage);
		}

		[Obsolete("Метод использует старый диалог вывода сообщения об ошибке. При переходе на класс UnhandledExceptionHandler, лучше не использовать старые сообщения.")]
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
		/// Главный цикл приложения будет вызываться не чаще чем время указанное в параметрах.
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
			
			StatusBarLabel.LabelProp = message.EllipsizeMiddle(160);
			if (!waitRedraw)
				return;		
			statusBarRedrawHandled = false;
			while (Application.EventsPending () && !statusBarRedrawHandled) {
				Gtk.Main.Iteration();
			}
		}

		[Obsolete("Для Gtk приложений пользуйтесь аналогичным функционалом из QS.ErrorReporting.GtkUI.UnhandledExceptionHandler без привязки к старой QSSupport.")]
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
						logger.Error("После аргумента {0} должно быть указано название соединения. А его нет.", args[i]);
						return;
					}

					i++;
					Login.OverwriteDefaultConnection = args[i];
				}
			}
		}
	}
}

