using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace QS.Dialog;

public class AvaloniaGuiDispatcher : IGuiDispatcher
{
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
	private static readonly TimeSpan commitTimeout = TimeSpan.FromSeconds(1);

	private static DateTime lastRedraw = DateTime.MinValue;

	public static Thread GuiThread;

	public AvaloniaGuiDispatcher()
	{
		// Сохраняем текущий поток как поток GUI
		GuiThread = Thread.CurrentThread;
	}

	Thread IGuiDispatcher.GuiThread => GuiThread;

	public void RunInGuiTread(Action action)
	{
		// Запускаем действие на потоке UI через Avalonia
		Dispatcher.UIThread.Post(() => action());
	}

	public void WaitInMainLoop(Func<bool> checkStop, uint sleepMilliseconds = 20)
	{
		if(checkStop())
			return;

		// Не поток GUI крутить главный цикл нельзя и не нужно, просто ждём
		if(!Dispatcher.UIThread.CheckAccess()) {
			while(!checkStop())
				Thread.Sleep((int)sleepMilliseconds);
			return;
		}

		var frame = new DispatcherFrame();
		var timer = new DispatcherTimer(
			TimeSpan.FromMilliseconds(sleepMilliseconds),
			DispatcherPriority.Background,
			(_, _) => {
				if(checkStop())
					frame.Continue = false;
			});
		timer.Start();
		try {
			Dispatcher.UIThread.PushFrame(frame);
		}
		finally {
			timer.Stop();
		}
	}

	public void WaitRedraw() => WaitForRedraw();

	public void WaitRedraw(int milliseconds) => WaitForRedraw(milliseconds);

	/// <summary>
	/// Аналог GtkHelper.WaitRedraw: дает интерфейсу отрисовать накопленные изменения, даже если поток GUI занят
	/// синхронной операцией. Нужен пока код должен работать и в Gtk, и в Avalonia.
	/// В Avalonia одной прокрутки очереди диспетчера недостаточно: отрисовку выполняет отдельный поток, которому изменения
	/// передаются пакетом (commit) по тику композитора, а пока поток GUI занят, тики не обрабатываются. Поэтому после
	/// обработки накопленных задач запрашиваем commit и крутим цикл событий, пока поток отрисовки его не примет.
	/// Вызывать нужно из потока GUI, из другого потока метод ничего не делает.
	/// </summary>
	/// <param name="minIntervalMilliseconds">Если с прошлой перерисовки прошло меньше этого времени, сразу выходим.</param>
	public static void WaitForRedraw(int minIntervalMilliseconds = 0)
	{
		if(!Dispatcher.UIThread.CheckAccess())
			return;

		if(DateTime.Now - lastRedraw < TimeSpan.FromMilliseconds(minIntervalMilliseconds))
			return;

		var timer = Stopwatch.StartNew();
		// Раскладка и прочие накопленные задачи, без них в пакет попадет старое состояние.
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

		var compositor = GetCompositor();
		if(compositor == null)
			return;

		var frame = new DispatcherFrame();
		var committed = false;
		// Предохранитель: если commit не завершается (окно скрыто или свернуто), не зависаем навсегда.
		DispatcherTimer.RunOnce(() => frame.Continue = false, commitTimeout, DispatcherPriority.Background);
		compositor.RequestCommitAsync().ContinueWith(_ => Dispatcher.UIThread.Post(() => {
			committed = true;
			frame.Continue = false;
		}, DispatcherPriority.Send), TaskScheduler.Default);
		Dispatcher.UIThread.PushFrame(frame);

		lastRedraw = DateTime.Now;
		if(committed)
			logger.Debug("Ожидание отрисовки заняло {0} мс.", timer.ElapsedMilliseconds);
		else
			logger.Warn("Отрисовка не завершилась за {0} мс, продолжаем без нее.", timer.ElapsedMilliseconds);
	}

	// Композитор общий для всех окон приложения, поэтому подойдет любое видимое окно.
	private static Compositor? GetCompositor()
	{
		var windows = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
		var window = windows?.LastOrDefault(x => x.IsVisible) ?? windows?.FirstOrDefault();
		return window == null ? null : ElementComposition.GetElementVisual(window)?.Compositor;
	}
}
