using System;
using System.Threading;
using Gtk;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorHandling.GtkUI
{
	/// <summary>
	/// Класс помогает сформировать отправку отчета о падении программы.
	/// Для работы необходимо предварительно сконфигурировать модуль
	/// GuiThread - указать поток Gui, нужно для корректной обработки эксепшенов в других потоках.
	/// ApplicationInfo - Передать класс возвращающий информация о програмамме
	/// Опционально:
	/// User - Текущий пользователь
	/// RequestEmail = true - требовать ввод E-mail
	/// RequestDescription = true - требовать ввода описания
	/// </summary>
	public static class UnhandledExceptionHandler
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Внешние настройки модуля

		public static Thread GuiThread;
		public static IApplicationInfo ApplicationInfo;
		public static UserBase User;
		public static bool RequestEmail = true;
		public static bool RequestDescription = true;

		#endregion

		private static ErrorMsgDlg currentCrashDlg;

		public static void SubscribeToUnhadledExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
				logger.Fatal((Exception)e.ExceptionObject, "Поймано необработаное исключение в Application Domain.");
				ErrorMessage((Exception)e.ExceptionObject);
			};
			GLib.ExceptionManager.UnhandledException += delegate (GLib.UnhandledExceptionArgs a) {
				logger.Fatal((Exception)a.ExceptionObject, "Поймано необработаное исключение в GTK.");
				ErrorMessage((Exception)a.ExceptionObject);
			};
		}

		public static void ErrorMessage(Exception ex)
		{
			if(GuiThread == Thread.CurrentThread) {
				RealErrorMessage(ex);
			}
			else {
				logger.Debug("From Another Thread");
				Application.Invoke(delegate {
					RealErrorMessage(ex);
				});
			}
		}

		private static void RealErrorMessage(Exception exception)
		{
			if(currentCrashDlg != null) {
				logger.Debug("Добавляем исключение в уже созданное окно.");
				currentCrashDlg.AddAnotherException(exception);
			}
			else {
				logger.Debug("Создание окна отправки отчета о падении.");
				currentCrashDlg = new ErrorMsgDlg(exception, ApplicationInfo, User);
				currentCrashDlg.Run();
				currentCrashDlg.Destroy();
				currentCrashDlg = null;
				logger.Debug("Окно отправки отчета, уничтожено.");
			}
		}

	}
}
