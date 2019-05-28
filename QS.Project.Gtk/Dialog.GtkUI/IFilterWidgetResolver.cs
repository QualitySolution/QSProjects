using System;
using Gtk;
using QS.RepresentationModel;
namespace QS.Dialog.GtkUI
{
	public interface IFilterWidgetResolver
	{
		Widget Resolve(IJournalFilter filter);
	}

	public class DefaultFilterWidgetResolver : IFilterWidgetResolver
	{
		public Widget Resolve(IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}
			throw new InvalidCastException($"Ошибка приведения типа {nameof(IJournalFilter)} к типу {nameof(Widget)}.");
		}
	}
}
