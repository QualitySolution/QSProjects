using System;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {
	public class OperationRefused : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public OperationRefused(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var refusal = exception.FindExceptionTypeInInner<OperationRefusedException>();
			if(refusal == null)
				return false;

			interactiveMessage.ShowMessage(ImportanceLevel.Warning, refusal.Message, "Операция не выполнена");
			return true;
		}
	}
}
