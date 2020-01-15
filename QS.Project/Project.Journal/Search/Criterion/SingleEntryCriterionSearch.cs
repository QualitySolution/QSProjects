using System;
namespace QS.Project.Journal.Search.Criterion
{
	public class SingleEntryCriterionSearch : ICriterionSearch
	{
		public CriterionSearchModel CriterionSearchModel => searchViewModel.CriterionSearchModel;

		public SearchViewModelBase SearchViewModel => searchViewModel;

		private readonly SingleEntryCriterionSearchViewModel searchViewModel;

		public SingleEntryCriterionSearch(SingleEntryCriterionSearchViewModel searchViewModel)
		{
			this.searchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));
		}
	}
}
