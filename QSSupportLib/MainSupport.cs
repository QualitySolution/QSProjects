using System;
using System.Reflection;
using Gtk;
using QSProjectsLib;

namespace QSSupportLib
{
	public static class MainSupport
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		public static BaseParam BaseParameters;

		private static ErrorMsg currentCrashDlg;

		private static AppVersion projectVerion;

		public static AppVersion ProjectVerion {
			get {
				if (projectVerion == null)
					projectVerion = new AppVersion ();
				return projectVerion;
			}
			set {
				projectVerion = value;
			}
		}

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

		[Obsolete("Используйте вместо этой проверки класс CheckBaseVersion. (Удалить после 17.11.16)")]
		public static bool CheckVersion (Window Parent)
		{
			//Если найдены ошибки.
			if (CheckBaseVersion.Check ()) {
				if (CheckBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater)
					return false;
				MessageDialog error = new MessageDialog (Parent, DialogFlags.DestroyWithParent,
					                      MessageType.Warning, 
					                      ButtonsType.Close, 
					CheckBaseVersion.TextMessage);
				error.Run ();
				error.Destroy ();
				Environment.Exit (1);
				return false;
			}

			return true;
		}

		public static void TestVersion (Window Parent)
		{
			if (CheckBaseVersion.Check ()) {
				MessageDialog VersionError = new MessageDialog (Parent, DialogFlags.DestroyWithParent,
					                             MessageType.Warning, 
					                             ButtonsType.Close, 
												CheckBaseVersion.TextMessage);
				VersionError.Run ();
				VersionError.Destroy ();
				Environment.Exit (0);
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
				MainSupport.BaseParameters = new BaseParam (QSMain.connectionDB);
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