using System;
using QS.Tdi;

namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelector : ITdiTab, IDisposable
	{
		event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
		bool CanOpen(JournalEntityNodeBase node);
		/// <summary>
		///
		/// </summary>
		/// <param name="node"></param>
		/// <returns>Возвращает вкладку для редактирования существующей сущности</returns>
		ITdiTab GetTabToOpen(JournalEntityNodeBase node);
	}
}
