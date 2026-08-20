using System;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {
	public class NotEnoughRights : IErrorHandler
	{
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
