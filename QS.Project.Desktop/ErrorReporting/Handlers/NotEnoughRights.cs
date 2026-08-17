using System;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {

	/// <summary>
	/// Отказ нашей же проверки прав: текст в исключении уже написан для пользователя
	/// («Недостаточно прав для управления пользователями»). Программа отработала как
	/// задумано, отправлять отчёт не о чем.
	/// </summary>
	public class NotEnoughRights : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public NotEnoughRights(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var rightsEx = exception.FindExceptionTypeInInner<UnauthorizedAccessException>();
			if(rightsEx == null)
				return false;

			interactiveMessage.ShowMessage(ImportanceLevel.Warning, rightsEx.Message, "Недостаточно прав");
			return true;
		}
	}
}
