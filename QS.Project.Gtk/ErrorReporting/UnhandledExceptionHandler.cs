using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;
using QS.Dialog;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	/// <summary>
	/// Делегат для перехвата и отдельной обработки некоторых ошибок.
	/// Метод должне возвращать true, если ошибку он обработал сам 
	/// и ее больше не надо передавать вниз по списку зарегистированных обработчиков,
	/// вплодь до стандартного диалога отправки отчета об ошибке.
	/// </summary>
	public delegate bool CustomErrorHandler(Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage);
	/// <summary>
	/// Класс помогает сформировать отправку отчета о падении программы.
	/// Для работы необходимо предварительно сконфигурировать модуль
	/// GuiThread - указать поток Gui, нужно для корректной обработки эксепшенов в других потоках.
	/// InteractiveMessage - Класс позволяющий обработчикам выдать сообщение пользователю.
	/// ErrorReportingSettings - Класс настроек и параметров, необходимых для отправки ошибок через IErrorReporer
	/// ErrorDialogSettings - Класс настроек для диалога отправки ошибок
	/// ErrorReporter - Класс для отправки сообщений об ошибках
	/// </summary>
	public static class UnhandledExceptionHandler
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Внешние настройки модуля

		public static Thread GuiThread;
		public static IInteractiveMessage InteractiveMessage;
		public static IErrorReportingParametersFactory ErrorReportingParametersFactory;
		public static IErrorDialogSettings ErrorDialogSettings;
		public static IErrorReporter ErrorReporter;
		public static IApplicationInfo ApplicationInfo;

		/// <summary>
		/// В список можно добавить собственные обработчики ошибкок. Внимание! Порядок добавления обрабочиков важен,
		/// так как если ошибку обработает первый обработчик ко второму она уже не попадет.
		/// </summary>
		public static readonly List<CustomErrorHandler> CustomErrorHandlers = new List<CustomErrorHandler>();

		#endregion

		private static ErrorMsgDlg currentCrashDlg;

		public static void SubscribeToUnhadledExceptions(
			IErrorReportingParametersFactory errorReportingParametersFactory, 
			IErrorDialogSettings errorDialogSettings, 
			IErrorReporter errorReporter, 
			IApplicationInfo applicationInfo
		)
		{
			ErrorReportingParametersFactory = errorReportingParametersFactory ?? throw new ArgumentNullException(nameof(errorReportingParametersFactory));
			ErrorDialogSettings = errorDialogSettings ?? throw new ArgumentNullException(nameof(errorDialogSettings));
			ErrorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));

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
			var parameters = ErrorReportingParametersFactory.CreateParameters();
			parameters.Exceptions.Add(exception);

			var appInfo = ApplicationInfo;
	
			foreach(var handler in CustomErrorHandlers) {
				if(handler(exception, appInfo, parameters.User, InteractiveMessage))
					return;
			}
			if(currentCrashDlg != null) {
				logger.Debug("Добавляем исключение в уже созданное окно.");
				currentCrashDlg.AddAnotherException(exception);
			}
			else {
				logger.Debug("Создание окна отправки отчета о падении.");
				currentCrashDlg = new ErrorMsgDlg(parameters, ErrorDialogSettings, ErrorReporter);
				currentCrashDlg.Run();
				currentCrashDlg.Destroy();
				currentCrashDlg = null;
				logger.Debug("Окно отправки отчета, уничтожено.");
			}
		}
	}
}