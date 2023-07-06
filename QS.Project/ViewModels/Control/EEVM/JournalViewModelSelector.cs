using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Dialog;
using System;
using System.Linq;

namespace QS.ViewModels.Control.EEVM {
	public class JournalViewModelSelector<TEntity, TJournalViewModel> : IEntitySelector
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase 
	{
		protected readonly INavigationManager navigationManager;
		protected readonly Func<ITdiTab> GetParentTab;
		protected readonly DialogViewModelBase ParentViewModel;

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public JournalViewModelSelector(Func<ITdiTab> getParentTab, INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.GetParentTab = getParentTab ?? throw new ArgumentNullException(nameof(getParentTab));
		}

		public JournalViewModelSelector(DialogViewModelBase parentViewModel, INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		}

		public Type EntityType => typeof(TEntity);

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public virtual void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page;
			if(ParentViewModel != null)
				page = navigationManager.OpenViewModel<TJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave);
			else
				page = (navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(GetParentTab(), OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if (!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.TabName = dialogTitle;
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
		where TJournalFilterViewModel : IJournalFilterViewModel
	{
		private readonly Action<TJournalFilterViewModel> filterParams;

		public JournalViewModelSelector(
			DialogViewModelBase parentViewModel, 
			INavigationManager navigationManager, 
			Action<TJournalFilterViewModel> filterParams
			):base (parentViewModel, navigationManager)
		{
			this.filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public JournalViewModelSelector(
			Func<ITdiTab> getParentTab,
			INavigationManager navigationManager,
			Action<TJournalFilterViewModel> filterParams
			) : base(getParentTab, navigationManager) {
			this.filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		public override void OpenSelector(string dialogTitle = null) {
			IPage<TJournalViewModel> page;
			if(ParentViewModel != null)
				page = navigationManager.OpenViewModel<TJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave);
			else
				page = (navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(GetParentTab(), OpenPageOptions.AsSlave);
			if(page.ViewModel.JournalFilter is IJournalFilterViewModel filter)
				filter.SetAndRefilterAtOnce(filterParams);
			else 
				throw new InvalidCastException($"Для установки параметров, фильтр {page.ViewModel.JournalFilter.GetType()} должен является типом {typeof(IJournalFilterViewModel)}"); 
			
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if(!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.TabName = dialogTitle;
			//Сначала на всякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}
	}
}
