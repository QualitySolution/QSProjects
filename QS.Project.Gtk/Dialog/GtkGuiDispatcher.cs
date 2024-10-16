﻿using System;
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

		public void RunInGuiTread(System.Action action) {
			if(GuiThread == Thread.CurrentThread)
				action();
			else
				Application.Invoke((sender, e) => action());
		}

		public void WaitInMainLoop(Func<bool> checkStop, uint sleepMilliseconds = 20)
		{
			while(!checkStop()) {
				GtkHelper.WaitRedraw();
				Thread.Sleep((int)sleepMilliseconds);
			}
		}

		public void WaitRedraw()
		{
			GtkHelper.WaitRedraw();
		}
	}
}
