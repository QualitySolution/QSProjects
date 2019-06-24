using System;
using QS.Tdi;
using System.Collections;
namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelector : ITdiTab, IDisposable
	{
		bool IsActive { get; }
		Type EntityType { get; }
		IList Items { get; }
		IJournalSearch Search { get; }
		event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
	}
}
