using QS.Project.Journal;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Базовая view model для панели действий журнала
	/// </summary>
	public abstract class JournalActionsViewModelBase
	{
		/// <summary>
		/// View model журнала, к которому относятся действия
		/// </summary>
		public JournalViewModelBase Journal { get; set; }
	}
}
