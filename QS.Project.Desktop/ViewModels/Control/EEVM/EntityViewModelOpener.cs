using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class EntityViewModelOpener<TEntityViewModel> : IEntityDlgOpener
		where TEntityViewModel : DialogViewModelBase
	{
		private readonly INavigationManager navigationManager;
		private readonly DialogViewModelBase masterViewModel;

		public EntityViewModelOpener(INavigationManager navigationManager, DialogViewModelBase masterViewModel = null)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.masterViewModel = masterViewModel;
		}

		public void OpenEntityDlg(int id)
		{
			navigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(masterViewModel, EntityUoWBuilder.ForOpen(id));
		}
	}
}
