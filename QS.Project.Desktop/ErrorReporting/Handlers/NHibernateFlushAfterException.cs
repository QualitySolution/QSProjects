using System;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	public class NHibernateFlushAfterException : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public NHibernateFlushAfterException(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var nhEx = ExceptionHelper.FindExceptionTypeInInner<NHibernate.AssertionFailure>(exception);
			if(nhEx != null && nhEx.Message.Contains("don't flush the Session after an exception occurs")) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "В этом диалоге ранее произошла ошибка, в следствии ее программа не может " +
				                                                      "сохранить данные. Закройте этот диалог и продолжайте работу в новом.");
				return true;
			}
			return false;
		}
	}
}
