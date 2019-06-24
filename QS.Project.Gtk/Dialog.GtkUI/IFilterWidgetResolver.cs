using System;
using Gtk;
namespace QS.Dialog.GtkUI
{
	public interface IFilterWidgetResolver
	{
		Widget Resolve(RepresentationModel.IJournalFilter filter);
		Widget Resolve(Project.Journal.IJournalFilter filter);
	}

	public class DefaultFilterWidgetResolver : IFilterWidgetResolver
	{
		public Widget Resolve(RepresentationModel.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}
			throw new InvalidCastException($"Ошибка приведения типа {nameof(RepresentationModel.IJournalFilter)} к типу {nameof(Widget)}.");
		}

		public Widget Resolve(Project.Journal.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}
			throw new InvalidCastException($"Ошибка приведения типа {nameof(RepresentationModel.IJournalFilter)} к типу {nameof(Widget)}.");
		}
	}
}
