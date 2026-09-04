namespace QS.Dialog;
public class AvaloniaInteractiveService(AvaloniaInteractiveMessage interactiveMessage, AvaloniaInteractiveQuestion interactiveQuestion) : IInteractiveService {

	public bool Question(string message, string title = null) {
		return interactiveQuestion.Question(message, title);
	}

	public string Question(string[] buttons, string message, string title = null) {
		return interactiveQuestion.Question(buttons, message, title);
	}

	public void ShowMessage(ImportanceLevel level, string message, string title = null) {
		interactiveMessage.ShowMessage(level, message, title);
	}
}
