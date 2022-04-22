using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class EntityViewModelOpener<TEntityViewModel> : IEntityDlgOpener
		where TEntityViewModel : DialogViewModelBase
	{
		private readonly INavigationManager navigationManager;
		private readonly DialogViewModelBase masterViewModel;
		readonly Func<ITdiTab> getParrentTab;

		public EntityViewModelOpener(INavigationManager navigationManager, DialogViewModelBase masterViewModel = null)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.masterViewModel = masterViewModel;
		}

		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public EntityViewModelOpener(INavigationManager navigationManager, Func<ITdiTab> getParrentTab)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.getParrentTab = getParrentTab;
		}

		public void OpenEntityDlg(int id)
		{
			if(getParrentTab != null)
				(navigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TEntityViewModel, IEntityUoWBuilder>(getParrentTab(), EntityUoWBuilder.ForOpen(id));
			else
				navigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(masterViewModel, EntityUoWBuilder.ForOpen(id));
		}
	}
}
