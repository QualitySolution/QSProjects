using System;
using QS.Navigation;
using QS.Project.Domain;

namespace QS.ViewModels.Control.EEVM
{
	public class EntityViewModelOpener<TEntityViewModel> : IEntityDlgOpener
		where TEntityViewModel : IDialogViewModel
	{
		private readonly INavigationManager navigationManager;
		private readonly IDialogViewModel masterViewModel;

		public EntityViewModelOpener(INavigationManager navigationManager, IDialogViewModel masterViewModel = null)
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
