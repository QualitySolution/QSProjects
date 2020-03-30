using QS.ViewModels;

namespace QS.Project.Journal.Search
{
	public abstract class SearchViewModelBase : WidgetViewModelBase
	{
		public abstract SearchModel SearchModel { get; }

		public abstract void ManualSearchUpdate();

		public virtual void UpdateSearchModel(string[] values)
		{
			SearchModel.SearchValues = values;
			SearchModel.Update();
		}
	}
}
