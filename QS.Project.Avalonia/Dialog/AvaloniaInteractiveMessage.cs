namespace QS.Dialog;
public class AvaloniaInteractiveMessage : IInteractiveMessage {
	public void ShowMessage(ImportanceLevel level, string message, string title = null) {
		AvaloniaInteractiveQuestion.ShowModal(
			new[] { "Закрыть" },
			message,
			title ?? "Сообщение",
			level);
	}
}
