using System;
using Gtk;

namespace QS.Tdi.Gtk
{
	public interface ITDIWidgetResolver
	{
		Widget Resolve(ITdiTab tab);
	}
}
