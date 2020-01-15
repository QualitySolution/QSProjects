using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;

namespace QS.Project.Journal
{
	public class FilterableMultipleEntityJournalViewModelBase<TNode, TFilterViewModel> : MultipleEntityJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
		where TFilterViewModel : FilterViewModelBase<TFilterViewModel>
	{

		private TFilterViewModel filterViewModel;
		public TFilterViewModel FilterViewModel {
			get => filterViewModel;
			set {
				filterViewModel = value;
				Filter = filterViewModel;
			}
		}

		public FilterableMultipleEntityJournalViewModelBase(
			TFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices, 
			ICriterionSearch criterionSearch) : base(unitOfWorkFactory, commonServices, criterionSearch)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
