using System;
using System.Threading;
using Gtk;
using QS.Utilities;

namespace QS.Dialog
{
	public class GtkGuiDispatcher : IGuiDispatcher
	{
		public static Thread GuiThread;

		public GtkGuiDispatcher()
		{
		}

		Thread IGuiDispatcher.GuiThread => GuiThread;

		public void RunInGuiTread(System.Action action)
		{
			Application.Invoke((sender, e) => action());
		}

		public void WaitRedraw()
		{
			GtkHelper.WaitRedraw();
		}
	}
}
