using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.Project.Filter;
using QS.DomainModel.UoW;
using QS.Project.Journal.Search.Criterion;
using QS.Project.Journal.Search;

namespace Vodovoz.JournalViewModels
{
	public abstract class FilterableSingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode, TFilterViewModel, TSearchModel> : SingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode, TSearchModel>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
		where TEntityTab : class, ITdiTab
		where TFilterViewModel : FilterViewModelBase<TFilterViewModel>
		where TSearchModel : CriterionSearchModelBase
	{
		private TFilterViewModel filterViewModel;
		public TFilterViewModel FilterViewModel {
			get => filterViewModel;
			set {
				filterViewModel = value;
				Filter = filterViewModel;
			}
		}

		protected FilterableSingleEntityJournalViewModelBase(TFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, SearchViewModelBase<TSearchModel> searchViewModel) : base(unitOfWorkFactory, commonServices, searchViewModel)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
