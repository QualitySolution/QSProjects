using System;
using System.Collections.Generic;
using Gtk;

namespace QS.Utilities
{
	public static class GtkHelper
	{
		public static IEnumerable<Widget> EnumerateAllChildren(Container parent)
		{
			foreach(var child in parent.Children)
			{
				var container = child as Container;
				if(container != null)
				{
					foreach (var childOfChild in EnumerateAllChildren (container))
					{
						yield return childOfChild;
					}
				}
				yield return child;
			}
		}

		public static void WaitRedraw()
		{
			while (Application.EventsPending()) {
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
			if (DateTime.Now.Subtract(lastRedraw).Milliseconds < milliseconds)
				return;

			lastRedraw = DateTime.Now;
			while (Application.EventsPending()) {
				Gtk.Main.Iteration();
			}
		}
		
		public static Window GetParentWindow(Widget widget)
		{
			switch(widget) {
				case null:
					return null;
				case Window w:
					return w;
				default:
					return GetParentWindow(widget.Parent);
			}
		}
	}
}
