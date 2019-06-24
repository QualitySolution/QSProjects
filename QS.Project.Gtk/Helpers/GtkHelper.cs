using System;
using Gtk;

namespace QS.Helpers
{
	public static class GtkHelper
	{
		public static void WaitRedraw()
		{
			while(Application.EventsPending()) {
				Gtk.Main.Iteration();
			}
		}

		static DateTime lastRedraw;
		/// <summary>
		/// Главный цикл приложения будет вызываться не чаще чем время указанное в парамерах.
		/// </summary>
		/// <param name="milliseconds">Milliseconds.</param>
		public static void WaitRedraw(int milliseconds)
		{
			if(DateTime.Now.Subtract(lastRedraw).Milliseconds < milliseconds)
				return;

			lastRedraw = DateTime.Now;
			while(Application.EventsPending()) {
				Gtk.Main.Iteration();
			}
		}
	}
}
