using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting;

/// <summary>Показывает окно ошибки напрямую, без навигатора и разрешения View через контейнер.</summary>
public class AvaloniaErrorDialog : IUnexpectedErrorDialog {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
	private readonly IApplicationInfo application;
	private readonly IErrorReportingSettings settings;
	private readonly ILogService log;
	private readonly IUserInfo? user;
	private readonly IDataBaseInfo? database;
	private ErrorReportWindow? current;

	public AvaloniaErrorDialog(IApplicationInfo application, IErrorReportingSettings settings, ILogService log,
		IUserInfo? user = null, IDataBaseInfo? database = null) {
		this.application = application;
		this.settings = settings;
		this.log = log;
		this.user = user;
		this.database = database;
	}

	public void Show(Exception exception, string title) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Post(() => ShowOnUiThread(exception, title));
			return;
		}
		ShowOnUiThread(exception, title);
	}

	private void ShowOnUiThread(Exception exception, string title) {
		try {
			ShowCore(exception, title);
		} catch(Exception ex) {
			logger.Error(ex, "Не удалось показать окно отчёта об ошибке");
			try { new QS.Dialog.AvaloniaInteractiveMessage().ShowMessage(QS.Dialog.ImportanceLevel.Error, exception.ToString(), title); }
			catch(Exception fallbackException) { logger.Error(fallbackException, "Не удалось показать сообщение об ошибке"); }
		}
	}

	private void ShowCore(Exception exception, string title) {
		if(current != null) {
			current.AddException(exception);
			current.Activate();
			return;
		}
		var model = new ErrorReportViewModel(exception, application, user, settings, log, database);
		var window = new ErrorReportWindow(model) { Title = title };
		current = window;
		window.Closed += (_, _) => current = null;
		var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		var owner = lifetime?.MainWindow;
		try {
			if(owner?.IsVisible == true)
				_ = window.ShowDialog(owner);
			else
				window.Show();
		} catch {
			current = null;
			throw;
		}
	}
}
