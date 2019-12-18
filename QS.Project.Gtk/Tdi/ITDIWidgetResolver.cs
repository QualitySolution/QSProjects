using System;
using Gtk;
using QS.Navigation.GtkUI;

namespace QS.Tdi
{
	public interface ITDIWidgetResolver : IWidgetResolver
	{
		Widget Resolve(ITdiTab tab);
	}
}
