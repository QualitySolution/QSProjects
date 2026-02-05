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
		// Блокируем выполнение до тех пор, пока checkStop не вернет true
		while (!checkStop())
		{
			WaitRedraw();
			Thread.Sleep((int)sleepMilliseconds);
		}
	}

	public void WaitRedraw()
	{
		// Используем Dispatcher для принудительного обновления интерфейса
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
	}
}

