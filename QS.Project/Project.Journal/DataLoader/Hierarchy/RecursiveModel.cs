using System;
using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader.Hierarchy {
	public class RecursiveModel<TNode> {
		internal Func<IEnumerable<TNode>, IList<TNode>> RecursiveSourceFunc;
		public void SetRecursiveSource(Func<IEnumerable<TNode>, IList<TNode>> recursiveSourceFunc) {
			RecursiveSourceFunc = recursiveSourceFunc;
		}
	}
}
