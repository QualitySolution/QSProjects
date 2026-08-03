using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using QS.Project.Avalonia;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Dialog;

public class AvaloniaInteractiveQuestion : IInteractiveQuestion {
	public bool Question(string message, string? title = null) {
		if(Dispatcher.UIThread.CheckAccess())
			throw new InvalidOperationException(
				"Синхронный Question нельзя вызывать из UI-потока — это приведёт к дедлоку");
		return Ask(new[] { "Да", "Нет" }, message, title).GetAwaiter().GetResult() == "Да";
	}

	// Возвращает подпись нажатой кнопки, null — если пользователь закрыл окно крестиком
	public string? Question(string[] buttons, string message, string? title = null) {
		if(Dispatcher.UIThread.CheckAccess())
			throw new InvalidOperationException(
				"Синхронный Question нельзя вызывать из UI-потока — это приведёт к дедлоку");
		return Ask(buttons, message, title).GetAwaiter().GetResult();
	}

	private static Task<string?> Ask(string[] buttons, string message, string? title) {
		var tcs = new TaskCompletionSource<string?>();

		Dispatcher.UIThread.InvokeAsync(() =>
		{
			try {
				ShowQuestion(buttons, message, title, tcs);
			}
			catch(Exception ex) {
				tcs.TrySetException(ex);
			}
		});

		return tcs.Task;
	}

	private static void ShowQuestion(string[] buttons, string message, string? title, TaskCompletionSource<string?> tcs)
	{
		var dialogButtons = buttons.Select(label => new Button { Content = label }).ToArray();
		var window = new DialogWindow(message, title ?? "Вопрос", ImportanceLevel.Info, dialogButtons);
		window.HideCloseButton();

		foreach(var button in dialogButtons)
			button.Click += (_, _) =>
			{
				tcs.TrySetResult((string?)button.Content);
				window.Close();
			};

		window.Closed += (_, _) => tcs.TrySetResult(null);

		var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		var owner = lifetime?.Windows.FirstOrDefault(w => w.IsActive && w.IsVisible) ?? lifetime?.MainWindow;

		if(owner != null && owner.IsVisible)
			_ = window.ShowDialog(owner);
		else
			window.Show();
	}
}
