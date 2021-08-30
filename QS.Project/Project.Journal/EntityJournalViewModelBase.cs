using System;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace QS.Project.Journal
{
	public abstract class EntityJournalViewModelBase<TEntity, TEntityViewModel, TNode> : JournalViewModelBase
		where TEntity : class, IDomainObject
		where TEntityViewModel : DialogViewModelBase
		where TNode : class
	{
		#region Обязательные зависимости
		
		protected EntityJournalActionsViewModel EntityJournalActionsViewModel { get; }
		
		#endregion
		#region Опциональные зависимости
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion
		
		protected EntityJournalViewModelBase(
			EntityJournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService = null
			) : base(journalActionsViewModel, unitOfWorkFactory, interactiveService, navigationManager)
		{
			CurrentPermissionService = currentPermissionService;
			EntityJournalActionsViewModel = journalActionsViewModel ?? throw new ArgumentNullException(nameof(journalActionsViewModel));

			var dataLoader = new ThreadDataLoader<TNode>(unitOfWorkFactory);
			dataLoader.CurrentPermissionService = currentPermissionService;
			dataLoader.AddQuery<TEntity>(ItemsQuery);
			DataLoader = dataLoader;

			if(currentPermissionService != null && !currentPermissionService.ValidateEntityPermission(typeof(TEntity)).CanRead)
				throw new AbortCreatingPageException($"У вас нет прав для просмотра документов типа: {typeof(TEntity).GetSubjectName()}", "Невозможно открыть журнал");

			var names = typeof(TEntity).GetSubjectNames();
			if(!String.IsNullOrEmpty(names?.NominativePlural))
				TabName = names.NominativePlural.StringToTitleCase();

			UpdateOnChanges(typeof(TEntity));
			InitializeJournalActionsViewModel();
		}
		
		protected override void InitializeJournalActionsViewModel()
		{
			EntityJournalActionsViewModel.Initialize(typeof(TEntity), CreateEntityDialog, EditEntityDialog);
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT будет выполнятся также медленно как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		protected abstract IQueryOver<TEntity> ItemsQuery(IUnitOfWork uow);

		#region Предопределенные действия
		
		protected virtual void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
		}

		protected virtual void EditEntityDialog(object node)
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
		}
		
		#endregion
	}
}
