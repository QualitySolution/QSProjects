using System;
using Avalonia.Threading;
using QS.Launcher.Services;

namespace QS.Launcher.Services {
	public class AvaloniaUiThreadInvoker : IUiThreadInvoker {
		public void Post(Action action) {
			if(action == null) return;
			Dispatcher.UIThread.Post(action);
		}
	}
}
