using System;

namespace QS.Dialog
{
	/// <summary>
	/// Объединяет <see cref="IInteractiveMessage"/> и <see cref="IInteractiveQuestion"/> в один сервис.
	/// Не зависит от UI, вся платформенная логика находится в переданных реализациях.
	/// </summary>
	public class InteractiveService : IInteractiveService
	{
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;

		public InteractiveService(IInteractiveMessage interactiveMessage, IInteractiveQuestion interactiveQuestion)
		{
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
		}

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			interactiveMessage.ShowMessage(level, message, title);
		}

		public bool Question(string message, string title = null)
		{
			return interactiveQuestion.Question(message, title);
		}

		public string Question(string[] buttons, string message, string title = null)
		{
			return interactiveQuestion.Question(buttons, message, title);
		}
	}
}
