using System;
using System.Reflection;
using Gtk;
using QSProjectsLib;

namespace QSSupportLib
{
	public static class MainSupport
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static bool SendErrorRequestEmail = true;

		public static Action<Exception> HandleStaleObjectStateException;

		private static ErrorMsg currentCrashDlg;

		static MainSupport ()
		{
			QSMain.RunErrorMessageDlg += HandleRunErrorMessageDlg;
		}

		/// <summary>
		/// Пока пустая функция для вызова инициализации библиотеки заранее. Дабы вызвался статический конструктор.
		/// </summary>
		public static void Init()
		{
			
		}

		public static void TestVersion (Window parent)
		{
			if (CheckBaseVersion.Check ()) {
				CheckBaseVersion.ShowErrorMessage (parent);
			}
		}

		public static string GetTitle ()
		{
			System.Reflection.Assembly assembly = Assembly.GetCallingAssembly ();
			object[] att = assembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);

			return ((AssemblyTitleAttribute)att [0]).Title;
		}

		static void HandleRunErrorMessageDlg (object sender, QSMain.RunErrorMessageDlgEventArgs e)
		{
			if(HandleStaleObjectStateException != null 
			   && e.Exception.InnerException != null 
			   && e.Exception.InnerException.GetType().Name == "StaleObjectStateException"){
				HandleStaleObjectStateException(e.Exception.InnerException);
				return;
			}

			if(currentCrashDlg != null)
			{
				logger.Debug ("Добавляем исключение в уже созданное окно.");
				currentCrashDlg.AddAnotherException (e.Exception, e.UserMessage);
			}
			else
			{
				logger.Debug ("Создание окна отправки отчета о падении.");
				currentCrashDlg = new ErrorMsg (e.ParentWindow, e.Exception, e.UserMessage);
				currentCrashDlg.Run ();
				currentCrashDlg.Destroy ();
				currentCrashDlg = null;
				logger.Debug ("Окно отправки отчета, уничтожено.");
			}
		}

		public static void LoadBaseParameters ()
		{
			try {
				MainSupport.BaseParameters = new BaseParam (QSMain.ConnectionDB);
			} catch (MySql.Data.MySqlClient.MySqlException e) {
				logger.Fatal (e, "Не удалось получить информацию о версии базы данных.", e);
				MessageDialog BaseError = new MessageDialog (QSMain.ErrorDlgParrent, DialogFlags.DestroyWithParent,
					                          MessageType.Warning, 
					                          ButtonsType.Close, 
					                          "Не удалось получить информацию о версии базы данных.");
				BaseError.Run ();
				BaseError.Destroy ();
				Environment.Exit (0);
			}
		}
	}
}