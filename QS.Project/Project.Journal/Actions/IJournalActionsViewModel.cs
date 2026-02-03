using System;
namespace QS.Project.Journal.Actions
{
	/// <summary>
	/// Базовый интрефейс для работы ViewModel действий журнала. Для обращения самого журнала и внешних потребителей.
	/// </summary>
	public interface IJournalActionsViewModel
	{
		JournalViewModelBase MyJournal { set; }

	}
}
