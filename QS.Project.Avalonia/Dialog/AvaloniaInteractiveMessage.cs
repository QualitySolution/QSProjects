using QS.Dialog.Windows;

namespace QS.Dialog;
public class AvaloniaInteractiveMessage : IInteractiveMessage {
	public void ShowMessage(ImportanceLevel level, string message, string title = null) {
		AvaloniaInteractiveQuestion.ShowModal(
			new[] { "Закрыть" },
			message,
			title ?? "Сообщение",
			ToIcon(level));
	}

	private static MessageIcon ToIcon(ImportanceLevel level) => level switch {
		ImportanceLevel.Warning => MessageIcon.Warning,
		ImportanceLevel.Error => MessageIcon.Error,
		ImportanceLevel.Success => MessageIcon.Success,
		_ => MessageIcon.Info
	};
}
