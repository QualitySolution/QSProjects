using System;
using System.Threading;

namespace QS.Dialog {
	public class ServerThreadDispatcher : IMainThreadDispatcher 
	{
		public bool HasMainThread => false;

		public Thread MainThread => Thread.CurrentThread;

		public void RunInMainTread(Action action) {
			action.Invoke();
		}
	}
}
