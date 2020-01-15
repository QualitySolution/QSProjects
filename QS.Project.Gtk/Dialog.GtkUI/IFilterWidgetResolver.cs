using System;
using Gtk;
namespace QS.Dialog.GtkUI
{
	public interface IFilterWidgetResolver
	{
		Widget Resolve(RepresentationModel.IJournalFilter filter);
		Widget Resolve(Project.Journal.IJournalFilter filter);
	}
}
