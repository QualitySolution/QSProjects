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

		public FilterableMultipleEntityJournalViewModelBase(TFilterViewModel filterViewModel, ICommonServices commonServices) : base(commonServices)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
