using QS.DomainModel.Entity;

namespace QS.Project.Journal.DataLoader
{
	public interface IHierarchicalDataLoader : IDataLoader
	{
		void LoadData();
		void PreloadGrandchilds(object node);
	}
}