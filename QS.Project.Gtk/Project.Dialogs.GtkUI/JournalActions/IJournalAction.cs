using System;
namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public interface IJournalAction
	{
		string Title { get; }

		bool Sensetive { get; }

		void CheckSensitive(object[] selected);

		void Execute();
	}
}
