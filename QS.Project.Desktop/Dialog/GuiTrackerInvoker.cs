using System;
using System.Threading;

namespace QS.Dialog {
	public class GuiTrackerInvoker : ITrackerActionInvoker {
		private readonly IGuiDispatcher _guiDispatcher;

		public GuiTrackerInvoker(IGuiDispatcher guiDispatcher) {
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
		}

		public void Invoke(Action action, bool runInInvokedThread = false) {
			if(_guiDispatcher.GuiThread == Thread.CurrentThread || runInInvokedThread) {
				action.Invoke();
			}
			else {
				_guiDispatcher.RunInGuiTread(action);
			}
		}
	}
}
