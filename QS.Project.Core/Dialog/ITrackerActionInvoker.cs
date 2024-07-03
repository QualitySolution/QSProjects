using System;

namespace QS.Dialog {
	public interface ITrackerActionInvoker {
		void Invoke(Action action, bool runInInvokedThread = false);
	}
}
