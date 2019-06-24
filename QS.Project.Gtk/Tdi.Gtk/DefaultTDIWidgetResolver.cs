using System;
using Gtk;
using QS.Journal.GtkUI;

namespace QS.Tdi.Gtk
{
	public class DefaultTDIWidgetResolver : ITDIWidgetResolver
	{
		public Widget Resolve(ITdiTab tab)
		{
			if(JournalViewFactory.TryCreateView(out Widget widget, tab)) {
				return widget;
			}

			if(tab is Widget) {
				return (Widget)tab;
			}
			throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");
		}
	}
}
