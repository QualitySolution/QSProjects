using System;
using System.Reactive;
using QS.ErrorReporting;

namespace QS.Dialog;

/// <summary>
/// перехват ошибок, выброшенных внутри команд, исключение из тела команды ReactiveUI ловит сам и отдаёт в её ThrownExceptions.
/// заменяется разбором через <see cref="IErrorHandlingService"/>
/// </summary>
/// <remarks>
/// пара к <see cref="DispatcherExceptionHandler"/>, который ловит ошибки потока GUI вне команд.
/// вызывать в OnFrameworkInitializationCompleted сразу, без аргумента, и ещё раз после сборки контейнера — с <see cref="IErrorHandlingService"/>.
/// повторный вызов обновляет используемый сервис обработки ошибок.
/// команда, подписанная на ThrownExceptions, сюда не попадает.
/// </remarks>
public static class RxAppExceptionHandler {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
	private static IErrorHandlingService? errorHandlingService;

	public static IObserver<Exception> Handler { get; } = Observer.Create<Exception>(Handle);

	/// <param name="errorHandling">
	/// цепочка обработчиков разбора ошибки и отправка отчёта. null - контейнер ещё не собран, пишем в лог и показываем сообщение
	/// </param>
	public static void Install(IErrorHandlingService? errorHandling = null) {
		errorHandlingService = errorHandling;

		if(errorHandling == null) {
			logger.Debug("Обработчик ошибок недоступен, ставим показ сообщения без разбора");
		}

	}

	private static void Handle(Exception ex) {
		if(errorHandlingService != null)
			errorHandlingService.Handle(ex);
		else
			ShowWithoutHandling(ex);
	}

	private static void ShowWithoutHandling(Exception ex) {
		logger.Error(ex, "Необработанная ошибка в ReactiveUI-команде.");
		new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Error, ex.Message, "Непредвиденная ошибка");
	}
}
