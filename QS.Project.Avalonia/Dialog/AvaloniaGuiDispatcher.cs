using System;
using System.Threading;
using Avalonia.Threading;

namespace QS.Dialog;

public class AvaloniaGuiDispatcher : IGuiDispatcher
{
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

	public void WaitRedraw()
	{
		// Используем Dispatcher для принудительного обновления интерфейса
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
	}
}

