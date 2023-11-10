using System;
using System.Threading;
using QS.Dialog;

namespace QS.Testing.Gui
{
	public class GuiDispatcherForTests : IGuiDispatcher
	{

		public Thread GuiThread => throw new NotImplementedException();

		public Thread MainThread => throw new NotImplementedException();

		public bool HasMainThread => throw new NotImplementedException();

		public void RunInGuiTread(Action action)
		{
			//Для тестирования ViewModel команды можно выполнять в том же потоке.
			//FIXME для тестирования View необходимо реализовать получение потока в котором нужно выполнять команды.
			action();
		}

		public void RunInMainTread(Action action) {
			RunInGuiTread(action);
		}

		public void WaitInMainLoop(Func<bool> checkStop, uint sleepMilliseconds = 20)
		{
			throw new NotImplementedException();
		}

		public void WaitRedraw()
		{
			throw new NotImplementedException();
		}
	}
}
