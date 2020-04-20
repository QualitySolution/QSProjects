using System;
using Gtk;
using QS.ViewModels;
namespace QS.Dialog.GtkUI
{
	[Obsolete("Используется только в ВОДОВОЗЕ. Удалить когда в водовозе не останется классов.")]
	public interface IFilterWidgetResolver
	{
		Widget Resolve(RepresentationModel.IJournalFilter filter);
		Widget Resolve(Project.Journal.IJournalFilter filter);
	}

	public interface IFooterWidgetResolver
	{
		Widget Resolve(ViewModelBase footer);
	}
}
