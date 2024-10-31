using Avalonia.Threading;
using QS.Dialog;

namespace QS.Project.Avalonia.Interactive;
public class AvaloniaInteractiveMessage : IInteractiveMessage {
	public void ShowMessage(ImportanceLevel level, string message, string title = null) {
		Dispatcher.UIThread.Post(() => DialogWindow.Show(level, message, title));
	}
}