using Gtk;
using QS.Project.Journal.Actions.ViewModels;
using QS.ViewModels;

namespace QS.Dialog
{
	public interface IJournalActionsResolver
	{
		Widget Resolve(JournalActionsViewModel journalActionsViewModel);
	}
}