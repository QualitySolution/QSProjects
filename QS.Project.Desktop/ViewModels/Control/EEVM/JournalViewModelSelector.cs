using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Dialog;
using System;
using System.Linq;

namespace QS.ViewModels.Control.EEVM {
	public class JournalViewModelSelector<TEntity, TJournalViewModel> : IEntitySelector
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase 
	{
		protected readonly INavigationManager navigationManager;
		protected readonly DialogViewModelBase ParentViewModel;

		public JournalViewModelSelector(DialogViewModelBase parentViewModel, INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		}

		public Type EntityType => typeof(TEntity);

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public virtual void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page = navigationManager.OpenViewModel<TJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave);
			
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if (!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.Title = dialogTitle;
			//Сначала на всякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		protected void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.SelectedObjects.First()));
		}

	}

	public class JournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel> : JournalViewModelSelector<TEntity, TJournalViewModel>
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase
		where TJournalFilterViewModel : class, IJournalFilterViewModel
	{
		private readonly Action<TJournalFilterViewModel> filterParams;
		private readonly TJournalFilterViewModel _filter;

		public JournalViewModelSelector(
			DialogViewModelBase parentViewModel,
			INavigationManager navigationManager,
			TJournalFilterViewModel filter
			) : base(parentViewModel, navigationManager) {
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public JournalViewModelSelector(
			DialogViewModelBase parentViewModel, 
			INavigationManager navigationManager, 
			Action<TJournalFilterViewModel> filterParams
			):base (parentViewModel, navigationManager)
		{
			this.filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		public override void OpenSelector(string dialogTitle = null) {
			IPage<TJournalViewModel> page;
			if(_filter != null) {
				page = navigationManager.OpenViewModel<TJournalViewModel, TJournalFilterViewModel>(ParentViewModel, _filter, OpenPageOptions.AsSlave);
			}
			else {
				page = navigationManager.OpenViewModel<TJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave);
			}

			if(page.ViewModel.JournalFilter != null) {
				if(page.ViewModel.JournalFilter is IJournalFilterViewModel filter) {
					if(filterParams != null) {
						filter.SetAndRefilterAtOnce(filterParams);
					}
				}
				else
					throw new InvalidCastException($"Для установки параметров, фильтр {page.ViewModel.JournalFilter.GetType()} должен является типом {typeof(IJournalFilterViewModel)}");
			}

			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if(!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.Title = dialogTitle;
			//Сначала на всякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}
	}
}
