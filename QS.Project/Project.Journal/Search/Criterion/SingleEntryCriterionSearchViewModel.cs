using System;
namespace QS.Project.Journal.Search.Criterion
{
	public class SingleEntryCriterionSearchViewModel : SingleEntrySearchViewModel
	{
		public override SearchModel SearchModel => CriterionSearchModel;

		public virtual CriterionSearchModelBase CriterionSearchModel { get; }

		public SingleEntryCriterionSearchViewModel(CriterionSearchModelBase criterionSearchModel) : base(criterionSearchModel)
		{
			CriterionSearchModel = criterionSearchModel ?? throw new ArgumentNullException(nameof(criterionSearchModel));
		}
	}
}
