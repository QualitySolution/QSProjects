﻿using System;
using System.Threading;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace QS.Project.Services
{
	public class DeleteEntityGUIService : IDeleteEntityService
	{
		private readonly DeleteConfiguration configuration;
		private readonly INavigationManager navigation;
		private readonly IInteractiveQuestion interactive;

		public DeleteEntityGUIService(DeleteConfiguration configuration, INavigationManager navigation, IInteractiveQuestion interactive)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		public DeleteCore DeleteEntity<TEntity>(int id, IUnitOfWork uow = null, Action beforeDeletion = null)
		{
			return DeleteEntity(typeof(TEntity), id, uow, beforeDeletion);
		}

		public DeleteCore DeleteEntity(Type clazz, int id, IUnitOfWork uow = null, Action beforeDeletion = null)
		{
			var deletion = new DeleteCore(configuration, uow);
			#region Подготовка удаления
			using(var cancellation = new CancellationTokenSource()) {
				var preparePage = navigation.OpenViewModel<PrepareDeletionViewModel, DeleteCore, CancellationTokenSource>(null, deletion, cancellation);
				deletion.PrepareDeletion(clazz, id, cancellation.Token);
				if(cancellation.IsCancellationRequested)
					return deletion;
				navigation.ForceClosePage(preparePage, CloseSource.External);
			}
			#endregion
			#region Диалог удаления
			if(deletion.TotalLinks > 0) {
				var deletionPage = navigation.OpenViewModel<DeletionViewModel, DeleteCore>(null, deletion);
				deletionPage.ViewModel.DeletionAccepted = () => RunDeletion(deletion);
			}
			else if(interactive.Question($"Удалить {deletion.RootEntity.Title}?"))
				RunDeletion(deletion);
			else
				deletion.DeletionExecuted = false;
			#endregion
			return deletion;
		}

		private void RunDeletion(DeleteCore deletion)
		{
			using(var cancellation = new CancellationTokenSource()) {
				var progressPage = navigation.OpenViewModel<DeletionProcessViewModel, DeleteCore, CancellationTokenSource>(null, deletion, cancellation);
				deletion.RunDeletion(cancellation.Token);
				if(cancellation.IsCancellationRequested)
					return;
				navigation.ForceClosePage(progressPage, CloseSource.External);
			}
		}
	} 
}
