namespace QS.Project.Journal.Search.Criterion
{
	public interface ICriterionSearch
	{
		CriterionSearchModel CriterionSearchModel { get; }
		SearchViewModelBase SearchViewModel { get; }
	}
}
