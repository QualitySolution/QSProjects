using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QS.ViewModels;

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
			EntitiesJournalActionsViewModel journalActionsViewModel,
			TFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(journalActionsViewModel, unitOfWorkFactory, commonServices)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
