using System;
using QS.Tdi;

namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelector : ITdiTab, IDisposable
	{
		event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
	}
}
