using System;
using System.Reactive;
using ReactiveUI;

namespace QS.Dialog;

public static class RxAppExceptionHandler {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	public static void Install() {
		RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex => {
			logger.Error(ex, "Необработанная ошибка в ReactiveUI-команде.");
			new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Error, ex.Message, "Непредвиденная ошибка");
		});
	}
}
