using System;
namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public interface IJournalAction
	{
		string Title { get; }

		bool Sensetive { get; }

		void CheckSensetive(object[] selected);

		void Execute();
	}
}
