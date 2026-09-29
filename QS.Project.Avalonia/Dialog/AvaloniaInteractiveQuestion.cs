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
		ShowModal(new[] { "Да", "Нет" }, message, title, ImportanceLevel.Info) == "Да";

	// Возвращает подпись нажатой кнопки, null — если пользователь закрыл окно крестиком
	public string? Question(string[] buttons, string message, string? title = null) =>
		ShowModal(buttons, message, title, ImportanceLevel.Info);

	/// <summary>Ждёт ответа пользователя, с какого бы потока вопрос ни задали</summary>
	internal static string? ShowModal(
		string[] buttons,
		string message,
		string? title,
		ImportanceLevel importanceLevel) {
		var answer = Show(buttons, message, title, importanceLevel);

		if(Dispatcher.UIThread.CheckAccess()) {
			var frame = new DispatcherFrame();
			_ = answer.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false),
				TaskScheduler.Default);
			Dispatcher.UIThread.PushFrame(frame);
		}

		return answer.GetAwaiter().GetResult();
	}

	private static Task<string?> Show(
		string[] buttons,
		string message,
		string? title,
		ImportanceLevel importanceLevel) {
		var tcs = new TaskCompletionSource<string?>();

		Dispatcher.UIThread.Post(() => {
			try {
				ShowDialog(buttons, message, title, importanceLevel, tcs);
			}
			catch(Exception ex) {
				tcs.TrySetException(ex);
			}
		}, DispatcherPriority.Background);

		return tcs.Task;
	}

	private static void ShowDialog(
		string[] buttons,
		string message,
		string? title,
		ImportanceLevel importanceLevel,
		TaskCompletionSource<string?> tcs)
	{
		var dialogButtons = buttons.Select(label => new Button { Content = label }).ToArray();
		var window = new DialogWindow(message, title ?? "Вопрос", importanceLevel, dialogButtons);
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
