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
	public bool Question(string message, string? title = null) =>
		Ask(new[] { "Да", "Нет" }, message, title) == "Да";

	// Возвращает подпись нажатой кнопки, null — если пользователь закрыл окно крестиком
	public string? Question(string[] buttons, string message, string? title = null) =>
		Ask(buttons, message, title);

	/// <summary>Ждёт ответа пользователя, с какого бы потока вопрос ни задали</summary>
	private static string? Ask(string[] buttons, string message, string? title) {
		var answer = Show(buttons, message, title);

		if(Dispatcher.UIThread.CheckAccess()) {
			var frame = new DispatcherFrame();
			_ = answer.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false),
				TaskScheduler.Default);
			Dispatcher.UIThread.PushFrame(frame);
		}

		return answer.GetAwaiter().GetResult();
	}

	private static Task<string?> Show(string[] buttons, string message, string? title) {
		var tcs = new TaskCompletionSource<string?>();

		Dispatcher.UIThread.Post(() => {
			try {
				ShowQuestion(buttons, message, title, tcs);
			}
			catch(Exception ex) {
				tcs.TrySetException(ex);
			}
		}, DispatcherPriority.Background);

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
		var owner = lifetime?.Windows.LastOrDefault(w => w.IsVisible) ?? lifetime?.MainWindow;

		// ShowDialog требует показанного владельца, а MainWindow из фолбэка может быть ещё не показан
		if(owner != null && owner.IsVisible)
			_ = window.ShowDialog(owner);
		else
			window.Show();
	}
}
