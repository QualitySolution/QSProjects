using System;
using Gtk;
namespace QS.Dialog.GtkUI
{
	[Obsolete("Используется только в ВОДОВОЗЕ. Удалить когда в водовозе не останется классов.")]
	public interface IFilterWidgetResolver
	{
		Widget Resolve(RepresentationModel.IJournalFilter filter);
		Widget Resolve(Project.Journal.IJournalFilter filter);
	}
}
