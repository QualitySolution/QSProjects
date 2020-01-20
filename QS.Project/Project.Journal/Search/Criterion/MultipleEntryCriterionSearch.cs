using System;
namespace QS.Project.Journal.Search.Criterion
{
	public class MultipleEntryCriterionSearch : ICriterionSearch
	{
		public CriterionSearchModelBase CriterionSearchModel => searchViewModel.CriterionSearchModel;

		public SearchViewModelBase SearchViewModel => searchViewModel;

		private readonly MultipleEntryCriterionSearchViewModel searchViewModel;

		public MultipleEntryCriterionSearch(MultipleEntryCriterionSearchViewModel searchViewModel)
		{
			this.searchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));
		}
	}
}
