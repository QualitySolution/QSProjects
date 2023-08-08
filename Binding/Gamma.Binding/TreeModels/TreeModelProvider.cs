using Gamma.Binding;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections;

namespace Gamma.TreeModels {
	public class TreeModelProvider : ITreeModelProvider {
		public IyTreeModel GetTreeModel(object datasource, bool reordable = false) {
			if(!(datasource is IList list)) {
				throw new NotSupportedException($"Type '{datasource.GetType()}' is not supported. Data source must implement IList.");
			}

			if(datasource is IObservableList observableList) {
				if(reordable) {
					return new ObservableListReorderableTreeModel(observableList);
				}
				else {
					return new ObservableListTreeModel(observableList);
				}
			}
			
			return new ListTreeModel(list);
		}
	}
}
