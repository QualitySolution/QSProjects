using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class JournalViewModelSelector<TEntity, TJournalViewModel> : IEntitySelector
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase 
	{
		protected readonly INavigationManager navigationManager;
		private readonly IUnitOfWork uow;
		protected readonly Func<ITdiTab> getParrentTab;
		protected readonly DialogViewModelBase parrentViewModel;

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public JournalViewModelSelector(Func<ITdiTab> getParrentTab, IUnitOfWork unitOfWork, INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.getParrentTab = getParrentTab ?? throw new ArgumentNullException(nameof(getParrentTab)); ;
		}

		public JournalViewModelSelector(DialogViewModelBase parrentViewModel, IUnitOfWork unitOfWork, INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.parrentViewModel = parrentViewModel ?? throw new ArgumentNullException(nameof(parrentViewModel)); ;
		}

		public Type EntityType => typeof(TEntity);

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public virtual void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page;
			if(parrentViewModel != null)
				page = navigationManager.OpenViewModel<TJournalViewModel>(parrentViewModel, OpenPageOptions.AsSlave);
			else
				page = (navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(getParrentTab(), OpenPageOptions.AsSlave);
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
		where TJournalFilterViewModel : JournalFilterViewModelBase<TJournalFilterViewModel> {
		private readonly Action<TJournalFilterViewModel> filterParams;

		public JournalViewModelSelector(DialogViewModelBase parrentViewModel, IUnitOfWork unitOfWork, INavigationManager navigationManager, Action<TJournalFilterViewModel> filterParams)
			:base (parrentViewModel, unitOfWork, navigationManager){
			this.filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		public override void OpenSelector(string dialogTitle = null) {
			IPage<TJournalViewModel> page;
			if(parrentViewModel != null)
				page = navigationManager.OpenViewModel<TJournalViewModel>(parrentViewModel, OpenPageOptions.AsSlave);
			else
				page = (navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(getParrentTab(), OpenPageOptions.AsSlave);
			if(page.ViewModel.JournalFilter is JournalFilterViewModelBase<TJournalFilterViewModel> filter)
				filter.SetAndRefilterAtOnce(filterParams);
			else 
				throw new InvalidCastException($"Для установки параметров фильтр журнала {page.ViewModel.JournalFilter.GetType()} должен является типом {typeof(JournalFilterViewModelBase<TJournalFilterViewModel>)}"); 
			
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if(!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.TabName = dialogTitle;
			//Сначала на всякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}
	}
}
