using System.Collections.ObjectModel;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Базовый интерфейс для доступа к действиям журнала (не дженерик)
	/// </summary>
	public interface IButtonJournalActionsViewModel
	{
		/// <summary>
		/// Коллекция действий для отображения
		/// </summary>
		ObservableCollection<IJournalActionView> ActionsView { get; }
	}
}
