using System;
namespace QS.Project.Journal.Search.Criterion
{
	public class MultipleEntryCriterionSearchViewModel : MultipleEntrySearchViewModel
	{
		public override SearchModel SearchModel => CriterionSearchModel;

		public virtual CriterionSearchModel CriterionSearchModel { get; }

		public MultipleEntryCriterionSearchViewModel(CriterionSearchModel criterionSearchModel) : base(criterionSearchModel)
		{
			CriterionSearchModel = criterionSearchModel ?? throw new ArgumentNullException(nameof(criterionSearchModel));
		}
	}
}
