using QS.Dialog;

namespace QS.Project.Services.Interactive
{
	public class ConsoleInteractiveService : IInteractiveService
	{
		private IInteractiveMessage interactiveMessage = new ConsoleInteractiveMessage();

		private IInteractiveQuestion interactiveQuestion = new ConsoleInteractiveQuestion();

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
