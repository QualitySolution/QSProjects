using System;
using System.Collections.Generic;
using Autofac;
using GLib;
using Gtk;
using QS.Dialog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Versioning;
using QS.Services;
using Thread = System.Threading.Thread;

namespace QS.ErrorReporting
{
	/// <summary>
	/// Класс помогает перехватывать не обработанные исключения и сформировать отправку отчета о падении программы.
	/// GtkGuiDispatcher.GuiThread - указать поток Gui, нужно для корректной обработки исключений в других потоках.
	/// </summary>
	public class UnhandledExceptionHandler : IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Зависимости 
		IApplicationInfo applicationInfo;
		IDataBaseInfo dataBaseInfo;
		ILogService logService;
		UserBase user;
		IErrorReportingSettings errorReportingSettings;
		#endregion
		
		/// <summary>
		/// Через контейнер будут получатся все дополнительные обработчики исключений IErrorHandler.
		/// Внимание! Порядок добавления обработчиков важен, так как если ошибку обработает первый обработчик ко второму она уже не попадет.
		/// </summary>
		IEnumerable<IErrorHandler> customErrorHandlers;
		
		private ErrorMsgDlg currentCrashDlg;

		/// <summary>
		/// Подписываемся на необработанные исключений
		/// </summary>
		public void SubscribeToUnhandledExceptions() {
			AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;
			GLib.ExceptionManager.UnhandledException += OnExceptionManagerOnUnhandledException;
		}

		/// <summary>
		/// Получаем из контейнера все необходимые для работы зависимости. Чтобы в момент аварии не к чему лишнему не обращаться, не лезть в базу и т.п.
		/// Контейнер не сохраняется. Метод можно вызывать повторно для переключения зависимостей на другой контейнер.
		/// </summary>
		public void UpdateDependencies(ILifetimeScope container, ProgressPerformanceHelper prpgress = null) {
			prpgress?.CheckPoint(nameof(applicationInfo));
			applicationInfo = container.Resolve<IApplicationInfo>();
			prpgress?.CheckPoint(nameof(errorReportingSettings));
			errorReportingSettings = container.Resolve<IErrorReportingSettings>();
			prpgress?.CheckPoint(nameof(logService));
			logService = container.Resolve<ILogService>();
			
			prpgress?.CheckPoint(nameof(customErrorHandlers));
			customErrorHandlers = container.Resolve<IEnumerable<IErrorHandler>>();
			prpgress?.CheckPoint(nameof(dataBaseInfo));
			dataBaseInfo = container.ResolveOptional<IDataBaseInfo>();
			
			prpgress?.CheckPoint(nameof(user));
			var userService = container.ResolveOptional<IUserService>();
			user = userService?.GetCurrentUser();
		}

		private void OnExceptionManagerOnUnhandledException(UnhandledExceptionArgs a) {
			logger.Fatal((Exception)a.ExceptionObject, "Поймано необработаное исключение в GTK.");
			ErrorMessage((Exception)a.ExceptionObject);
		}

		private void OnCurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
			logger.Fatal((Exception)e.ExceptionObject, "Поймано необработаное исключение в Application Domain.");
			ErrorMessage((Exception)e.ExceptionObject);
		}

		private void ErrorMessage(Exception ex)
		{
			if(GtkGuiDispatcher.GuiThread == Thread.CurrentThread) {
				RealErrorMessage(ex);
			}
			else {
				logger.Debug("From Another Thread");
				Application.Invoke(delegate {
					RealErrorMessage(ex);
				});
			}
		}

		private void RealErrorMessage(Exception exception)
		{
			foreach(var handler in customErrorHandlers) {
				try {
					if(handler.Take(exception)) {
						return;
					}
				}
				catch(Exception ex) {
					logger.Error(ex, "Ошибка в CustomErrorHandler");
				}
			}

			if(currentCrashDlg != null) {
				logger.Debug("Добавляем исключение в уже созданное окно.");
				currentCrashDlg.AddAnotherException(exception);
			}
			else {
				logger.Debug("Создание окна отправки отчета о падении.");
				currentCrashDlg = new ErrorMsgDlg(exception, applicationInfo, user, errorReportingSettings, logService, dataBaseInfo);
				currentCrashDlg.Run();
				currentCrashDlg.Destroy();
				currentCrashDlg = null;
				logger.Debug("Окно отправки отчета, уничтожено.");
			}
		}

		public void Dispose() {
			AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledException;
			GLib.ExceptionManager.UnhandledException -= OnExceptionManagerOnUnhandledException;
		}
	}
}
