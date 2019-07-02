using System;
using QS.Tdi;
using System.Collections;
namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelector : ITdiTab, IDisposable
	{
		event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
	}
}
