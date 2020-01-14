using System;
using Gtk;

namespace QS.Tdi
{
	public interface ITDIWidgetResolver
	{
		Widget Resolve(ITdiTab tab);
	}
}
