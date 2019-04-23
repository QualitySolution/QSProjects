using System;
using Gtk;

namespace QS.Tdi.Gtk
{
	public class TDIWidgetBasicResolver : ITDIWidgetResolver
	{
		public Widget Resolve(ITdiTab tab)
		{
			if(tab is Widget) {
				return (Widget)tab;
			}
			throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");
		}
	}
}
