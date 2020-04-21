using System;
using Gtk;
using QS.Journal.GtkUI;
using QS.Views.Resolve;

namespace QS.Tdi
{
	public class DefaultTDIWidgetResolver : ITDIWidgetResolver
	{
		public virtual Widget Resolve(ITdiTab tab)
		{
			if(tab is Widget) {
				return (Widget)tab;
			}

			IGtkViewResolver viewResolver = null;

			if(JournalViewFactory.TryCreateView(out Widget widget, tab, viewResolver)) {
				return widget;
			}

			return null;
		}
	}
}
