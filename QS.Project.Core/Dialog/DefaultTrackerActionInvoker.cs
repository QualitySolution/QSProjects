using System;

namespace QS.Dialog {
	public class DefaultTrackerActionInvoker : ITrackerActionInvoker {
		public void Invoke(Action action) {
			action.Invoke();
		}
	}
}
