using System.Collections.ObjectModel;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Базовый интерфейс для доступа к действиям журнала (не дженерик)
	/// </summary>
	public interface IButtonJournalActionsViewModel
	{
		/// <summary>
		/// Коллекция действий для отображения (слева)
		/// </summary>
		ObservableCollection<IJournalActionView> LeftActionsView { get; }
		
		/// <summary>
		/// Коллекция действий для отображения справа на панели
		/// </summary>
		ObservableCollection<IJournalActionView> RightActionsView { get; }
	}
}
