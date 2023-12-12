using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Filter;
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
			INavigationManager navigationManager = null) : base(unitOfWorkFactory, commonServices, navigationManager)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
