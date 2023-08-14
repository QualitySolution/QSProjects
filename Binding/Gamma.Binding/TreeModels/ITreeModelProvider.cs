using Gamma.Binding;

namespace Gamma.TreeModels {
	public interface ITreeModelProvider {

		IyTreeModel GetTreeModel(object datasource, bool reordable = false); 
	}
}
