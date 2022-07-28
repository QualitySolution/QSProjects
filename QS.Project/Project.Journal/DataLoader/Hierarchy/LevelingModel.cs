using System;
using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader.Hierarchy {
	public class LevelingModel<TNode> {
		internal List<Func<IEnumerable<TNode>, IList<TNode>>> LevelingFunctions { get; } = new List<Func<IEnumerable<TNode>, IList<TNode>>>();
		public LevelingModel<TNode> AddNextLevelSource(Func<IEnumerable<TNode>, IList<TNode>> queryFunc) {
			LevelingFunctions.Add(queryFunc);
			return this;
		}
	}
}
