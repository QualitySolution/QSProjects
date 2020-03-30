using System;
namespace QS.Project.Journal.Search
{
	public abstract class SearchViewModelBase<TSearchModel> : SearchViewModelBase
		where TSearchModel : SearchModel
	{
		public abstract TSearchModel SearchModelGeneric { get; }

		public override SearchModel SearchModel => SearchModelGeneric;
	}
}
