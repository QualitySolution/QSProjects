﻿using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;

namespace QS.Project.Journal.EntitySelection
{
	public class EntityViewModelOpener<TEntityViewModel> : IEntityDlgOpener
		where TEntityViewModel : ViewModelBase
	{
		private readonly INavigationManager navigationManager;
		private readonly ViewModelBase masterViewModel;

		public EntityViewModelOpener(INavigationManager navigationManager, ViewModelBase masterViewModel = null)
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
