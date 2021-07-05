using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class FilterableSingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode, TFilterViewModel>
		: SingleEntityJournalViewModelBase<TEntity, TEntityTab, TNode>
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

		protected FilterableSingleEntityJournalViewModelBase(
			JournalActionsViewModel journalActionsViewModel,
			TFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false) 
			: base(
				journalActionsViewModel,
				unitOfWorkFactory,
				commonServices,
				hideJournalForOpenDialog,
				hideJournalForCreateDialog)
		{
			FilterViewModel = filterViewModel;
		}
	}
}
