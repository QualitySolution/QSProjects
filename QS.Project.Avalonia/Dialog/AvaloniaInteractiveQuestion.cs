using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using QS.Project.Avalonia;

namespace QS.Dialog;

public class AvaloniaInteractiveQuestion : IInteractiveQuestion {

	public bool Question(string message, string title = null) {
		if(Dispatcher.UIThread.CheckAccess())
			throw new InvalidOperationException(
				"Синхронный Question нельзя вызывать из UI-потока — это приведёт к дедлоку. Используйте QuestionAsync.");
		return QuestionAsync(message, title).GetAwaiter().GetResult();
	}

	public string Question(string[] buttons, string message, string title = null) {
		if(Dispatcher.UIThread.CheckAccess())
			throw new InvalidOperationException(
				"Синхронный Question нельзя вызывать из UI-потока — это приведёт к дедлоку. Используйте QuestionAsync.");
		return QuestionAsync(buttons, message, title).GetAwaiter().GetResult();
	}

	public Task<bool> QuestionAsync(string message, string title = null) =>
		Dispatcher.UIThread.InvokeAsync(async () => {
			var dlg = new DialogWindow(message, title ?? string.Empty, ImportanceLevel.Info);
			var tcs = new TaskCompletionSource<bool>();

			var yes = new Button { Content = "Да" };
			var no = new Button { Content = "Нет" };
			yes.Click += (_, _) => { tcs.TrySetResult(true); dlg.Close(); };
			no.Click += (_, _) => { tcs.TrySetResult(false); dlg.Close(); };
			dlg.AddButton(yes);
			dlg.AddButton(no);
			// Закрытие крестиком / штатной кнопкой "Закрыть" => Нет
			dlg.Closed += (_, _) => tcs.TrySetResult(false);

			await ShowAsync(dlg);
			return await tcs.Task;
		});

	public Task<string> QuestionAsync(string[] buttons, string message, string title = null) =>
		Dispatcher.UIThread.InvokeAsync(async () => {
			var dlg = new DialogWindow(message, title ?? string.Empty, ImportanceLevel.Info);
			var tcs = new TaskCompletionSource<string>();

			foreach(var label in buttons) {
				var captured = label;
				var btn = new Button { Content = captured };
				btn.Click += (_, _) => { tcs.TrySetResult(captured); dlg.Close(); };
				dlg.AddButton(btn);
			}
			// Закрытие крестиком => null
			dlg.Closed += (_, _) => tcs.TrySetResult(null);

			await ShowAsync(dlg);
			return await tcs.Task;
		});

	private static Task ShowAsync(Window dlg) {
		var owner = GetOwner();
		if(owner != null && owner != dlg && owner.IsVisible)
			return dlg.ShowDialog(owner);
		dlg.Show();
		return Task.CompletedTask;
	}

	private static Window GetOwner() {
		if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			return desktop.MainWindow;
		return null;
	}
}
