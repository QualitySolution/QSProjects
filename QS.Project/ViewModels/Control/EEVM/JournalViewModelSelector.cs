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
		private readonly INavigationManager navigationManager;
		private readonly IUnitOfWork uow;
		readonly Func<ITdiTab> getParrentTab;
		readonly DialogViewModelBase parrentViewModel;

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Констуктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
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

		public void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page;
			if(parrentViewModel != null)
				page = navigationManager.OpenViewModel<TJournalViewModel>(parrentViewModel, OpenPageOptions.AsSlave);
			else
				page = (navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(getParrentTab(), OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if (!String.IsNullOrEmpty(dialogTitle))
				page.ViewModel.TabName = dialogTitle;
			//Сначала навсякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.SelectedObjects.First()));
		}

	}
}
