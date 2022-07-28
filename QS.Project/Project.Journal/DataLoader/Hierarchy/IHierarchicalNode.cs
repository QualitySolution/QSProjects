using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader.Hierarchy {
	public interface IHierarchicalNode<TNode>
		where TNode : class, IHierarchicalNode<TNode> {

		int Id { get; set; }
		int? ParentId { get; set; }
		TNode Parent { get; set; }
		IList<TNode> Children { get; set; }
	}
}
