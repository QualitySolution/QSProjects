using QS.Dialog;
using QS.Dialog.GtkUI;

namespace QS.Project.Services.GtkUI
{
	public class GtkInteractiveService : IInteractiveService
	{
		private IInteractiveMessage interactiveMessage = new GtkMessageDialogsInteractive();

		private IInteractiveQuestion interactiveQuestion = new GtkQuestionDialogsInteractive();

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
