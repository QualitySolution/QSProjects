using System;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.Tdi;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public interface IJournalDialog : ISingleUoWDialog, ITdiJournal
	{
		yTreeView TreeView { get; }

		JournalSelectMode Mode { get; }

		object[] SelectedNodes { get; }

		void OnObjectSelected(params object[] nodes);
	}
}
