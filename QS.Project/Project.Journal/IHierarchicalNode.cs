using System.Collections.Generic;

namespace QS.Project.Journal
{
	public interface IHierarchicalNode<TParent, TChild>
		where TParent : class
		where TChild : class
	{
		TParent Parent { get; set; }
		int? ParentId { get; set; }
		IList<TChild> Children { get; set; }
	}
}
