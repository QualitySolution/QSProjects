using QS.DomainModel.UoW;
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

		public FilterableMultipleEntityJournalViewModelBase(TFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
			FilterViewModel = filterViewModel;
		}
	}

	public class FilterableMultipleEntityJournalViewModelBase<TNode, TFilterViewModel, TFooterViewModel> : MultipleEntityJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
		where TFilterViewModel : FilterViewModelBase<TFilterViewModel>
		where TFooterViewModel : FooterViewModelBase<TFooterViewModel>
	{

		private TFilterViewModel filterViewModel;
		public TFilterViewModel FilterViewModel {
			get => filterViewModel;
			set {
				filterViewModel = value;
				Filter = filterViewModel;
			}
		}

		private TFooterViewModel footerViewModel;
		public TFooterViewModel FooterViewModel {
			get => footerViewModel;
			set {
				footerViewModel = value;
				Footer = footerViewModel;
			}
		}

		public FilterableMultipleEntityJournalViewModelBase(TFilterViewModel filterViewModel, 
															TFooterViewModel footerViewModel, 
															IUnitOfWorkFactory unitOfWorkFactory, 
															ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
			FilterViewModel = filterViewModel;
			FooterViewModel = footerViewModel;
		}
	}
}
