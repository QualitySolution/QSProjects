using System;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Utilities.Text;

namespace QS.Project.Journal
{
	public abstract class EntityJournalViewModelBase<TEntity, TNode> : JournalViewModelBase
		where TEntity : class, IDomainObject
		where TNode : class
	{
		#region Обязательные зависимости

		#endregion
		#region Опциональные зависимости
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion
		
		protected EntityJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			JournalActionsViewModel journalActionsViewModel = null,
			ICurrentPermissionService currentPermissionService = null
		) : base(unitOfWorkFactory, interactiveService, navigationManager, journalActionsViewModel)
		{
			CurrentPermissionService = currentPermissionService;

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
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT будет выполнятся также медленно как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		protected abstract IQueryOver<TEntity> ItemsQuery(IUnitOfWork uow);
	}
}
