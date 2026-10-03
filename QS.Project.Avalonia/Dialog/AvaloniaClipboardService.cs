using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace QS.Dialog;

/// <summary>
/// Помещает текст в буфер обмена через главное окно приложения.
/// </summary>
public class AvaloniaClipboardService : IClipboardService {
	public void SetText(string text) {
		Dispatcher.UIThread.Post(async () => {
			var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
			if(window?.Clipboard != null)
				await window.Clipboard.SetTextAsync(text);
		});
	}
}
