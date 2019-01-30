using System;
using Gtk;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public interface IJournalActionButton : IJournalAction
	{
		Button Button { get; }
	}
}
