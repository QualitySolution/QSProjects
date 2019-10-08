using System.ComponentModel;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.Project.Filter;

namespace Vodovoz.JournalViewModels
{
	public abstract class FilterableSingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode, TFilterViewModel> : SingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
		where TEntityTab : class, ITdiTab
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

		protected FilterableSingleEntityJournalViewModelBase(TFilterViewModel filterViewModel, ICommonServices commonServices) : base(commonServices)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
