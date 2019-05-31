using System;
using Gtk;
namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public interface IJournalPopupAction : IJournalAction
	{
		MenuItem MenuItem { get; }

		bool Visible { get; }

		void CheckVisibility(object[] selected);

		object[] SelectedItems { get; set; }
	}
}
