using Avalonia.Threading;
using QS.Dialog;
using QS.Project.Avalonia;

namespace QS.Project.Interactive;
public class AvaloniaInteractiveMessage : IInteractiveMessage {
	public void ShowMessage(ImportanceLevel level, string message, string title = null) {
		Dispatcher.UIThread.Post(() => DialogWindow.Show(level, message, title));
	}
}
