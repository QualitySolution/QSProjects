using Avalonia.Threading;
using QS.ErrorReporting;
using System;

namespace QS.Dialog;

/// <summary>
/// Ловит исключения потока интерфейса, идущие мимо ReactiveCommand
/// </summary>
public static class DispatcherExceptionHandler {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	private static IErrorHandlingService? errorHandling;
	private static bool installed;

	/// <param name="errorHandlingService">
	/// Разбор ошибки: цепочка обработчиков и отправка отчёта. null - контейнер ещё не собран,
	/// показываем голое сообщение
	/// </param>
	public static void Install(IErrorHandlingService? errorHandlingService = null) {
		errorHandling = errorHandlingService;
		if(installed)
			return;

		Dispatcher.UIThread.UnhandledException += OnUnhandled;
		installed = true;
	}

	private static void OnUnhandled(object? sender, DispatcherUnhandledExceptionEventArgs e) {
		try {
			var handling = errorHandling;
			if(handling != null)
				handling.Handle(e.Exception);
			else
				ShowWithoutHandling(e.Exception);
		}
		catch(Exception ex) {
			logger.Error(ex, "Не удалось разобрать ошибку потока интерфейса");
		}

		// без этого исключение уйдёт дальше в домен и закроет приложение
		e.Handled = true;
	}

	private static void ShowWithoutHandling(Exception ex) {
		logger.Error(ex, "Необработанная ошибка на потоке интерфейса.");
		new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Error, ex.Message, "Непредвиденная ошибка");
	}
}
