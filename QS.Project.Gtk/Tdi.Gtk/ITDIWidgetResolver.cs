using System;
using Gtk;
using QS.Navigation.GtkUI;

namespace QS.Tdi.Gtk
{
	public interface ITDIWidgetResolver : IWidgetResolver
	{
		Widget Resolve(ITdiTab tab);
	}
}
