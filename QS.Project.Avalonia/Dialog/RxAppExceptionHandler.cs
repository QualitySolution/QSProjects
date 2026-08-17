using System;
using System.Reactive;
using QS.ErrorReporting;
using ReactiveUI;

namespace QS.Dialog;

/// <summary>
/// Глобальная сеть под всё, что не поймали на месте. Ставится один раз при старте,
/// до создания первых ReactiveCommand: иначе они захватят стандартный обработчик
/// ReactiveUI, роняющий приложение.
/// </summary>
public static class RxAppExceptionHandler {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	/// <param name="errorHandling">
	/// Разбор ошибки: цепочка обработчиков и отправка отчёта. null - контейнер ещё не собран,
	/// показываем голое сообщение, потому что совсем без сети приложение просто упадёт.
	/// </param>
	public static void Install(IErrorHandlingService? errorHandling = null) {
		if(errorHandling == null) {
			logger.Debug("Обработчик ошибок недоступен, ставим показ сообщения без разбора");
			RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ShowWithoutHandling);
			return;
		}

		RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex => errorHandling.Handle(ex));
	}

	private static void ShowWithoutHandling(Exception ex) {
		logger.Error(ex, "Необработанная ошибка в ReactiveUI-команде.");
		new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Error, ex.Message, "Непредвиденная ошибка");
	}
}
