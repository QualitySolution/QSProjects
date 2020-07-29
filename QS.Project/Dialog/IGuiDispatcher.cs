using System;
using System.Threading;

namespace QS.Dialog
{
	public interface IGuiDispatcher
	{
		Thread GuiThread { get; }
		void WaitRedraw();
		void RunInGuiTread(Action action);
	}
}
