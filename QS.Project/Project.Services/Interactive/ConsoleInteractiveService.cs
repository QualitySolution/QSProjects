using QS.Dialog;

namespace QS.Project.Services.Interactive
{
	public class ConsoleInteractiveService : IInteractiveService
	{
		public ConsoleInteractiveService(IInteractiveMessage interactiveMessage, IInteractiveQuestion interactiveQuestion) {
			this.interactiveMessage = interactiveMessage ?? throw new System.ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new System.ArgumentNullException(nameof(interactiveQuestion));
		}

		private IInteractiveMessage interactiveMessage;

		private IInteractiveQuestion interactiveQuestion;

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
