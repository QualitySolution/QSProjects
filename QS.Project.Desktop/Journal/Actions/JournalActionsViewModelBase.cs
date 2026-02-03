using System.Collections.Generic;
using QS.Project.Journal;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Базовая view model для панели действий журнала
	/// </summary>
	/// <typeparam name="TNode">Тип узла (строки) журнала</typeparam>
	public abstract class JournalActionsViewModelBase<TNode> : IJournalEventsHandler
	{
		/// <summary>
		/// View model журнала, к которому относятся действия
		/// </summary>
		public JournalViewModelBase Journal { get; set; }

		#region IJournalEventsHandler

		public virtual void OnCellDoubleClick(object node, object columnTag, object subcolumnTag)
		{
		}

		public virtual void OnSelectionChanged(IList<object> nodes)
		{
		}

		public virtual void OnKeyPressed(string key)
		{
		}

		#endregion
	}
}
