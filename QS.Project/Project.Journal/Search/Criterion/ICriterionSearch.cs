namespace QS.Project.Journal.Search.Criterion
{
	public interface ICriterionSearch
	{
		CriterionSearchModelBase CriterionSearchModel { get; }
		SearchViewModelBase SearchViewModel { get; }
	}
}
