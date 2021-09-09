using System;
using QS.Tdi;

namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelector : ITdiTab, IDisposable
	{
		event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
		bool CanOpen(Type subjectType);
		/// <summary>
		///
		/// </summary>
		/// <returns>Возвращает вкладку для редактирования существующей сущности</returns>
		ITdiTab GetTabToOpen(Type subjectType, int subjectId);
	}
}
