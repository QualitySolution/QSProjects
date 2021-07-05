using Gtk;
using QS.ViewModels;

namespace QS.Dialog
{
	public interface IJournalActionsResolver
	{
		Widget Resolve(JournalActionsViewModel journalActionsViewModel);
	}
}