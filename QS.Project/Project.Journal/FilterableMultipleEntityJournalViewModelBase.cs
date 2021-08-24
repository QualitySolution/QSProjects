using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QS.Tdi;

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

		public override bool CanOpen()
		{
			return false;
		}

		public override ITdiTab GetTabToOpen(JournalEntityNodeBase node)
		{
			throw new System.NotImplementedException();
		}
	}
}
