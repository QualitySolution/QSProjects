using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.NodeExtensions;
using QS.Project.Services;
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
		#endregion
		#region Опциональные зависимости
		protected IDeleteEntityService DeleteEntityService;
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion

		protected EntityJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;

			var dataLoader = new ThreadDataLoader<TNode>(unitOfWorkFactory);
			dataLoader.CurrentPermissionService = currentPermissionService;
			dataLoader.AddQuery<TEntity>(ItemsQuery, ItemsCountFunction);
			DataLoader = dataLoader;

			if(currentPermissionService != null && !currentPermissionService.ValidateEntityPermission(typeof(TEntity)).CanRead)
				throw new AbortCreatingPageException($"У вас нет прав для просмотра документов типа: {typeof(TEntity).GetSubjectName()}", "Невозможно открыть журнал");

			CreateNodeActions();

			var names = typeof(TEntity).GetSubjectNames();
			if(!String.IsNullOrEmpty(names?.NominativePlural))
				TabName = names.NominativePlural.StringToTitleCase();

			UpdateOnChanges(typeof(TEntity));
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			
			bool canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(TEntity)).CanCreate;

			var addAction = new JournalAction("Создать",
					(selected) => canCreate,
					(selected) => VisibleCreateAction,
					(selected) => CreateEntityDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction( 
				    selected => CalculatePermission(selected).Any(p => p.CanUpdate) ? "Изменить" : "Открыть",
					(selected) => selected.Any() && CalculatePermission(selected).All(s => s.CanUpdate || s.CanRead),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<TNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
					(selected) => selected.Any() && CalculatePermission(selected).All(s => s.CanDelete),
					(selected) => VisibleDeleteAction,
					(selected) => DeleteEntities(selected.Cast<TNode>().ToArray()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);
		}

		private IEnumerable<IPermissionResult> CalculatePermission(object[] selected){
			if(CurrentPermissionService == null) {
				yield return new SimplePermissionResult(true, true, true, true);
			}
			//Если нода журнала может сообщать дату документа запускаем проверку каждого элемента
			else if(typeof(TNode).IsAssignableFrom(typeof(IDatedJournalNode))) {
				foreach(var node in selected) 
					yield return CurrentPermissionService.ValidateEntityPermission(typeof(TEntity), ((IDatedJournalNode)node).DocumentDate);
			}
			else { //Если нода обычная достаточно проверки по типу
				yield return CurrentPermissionService.ValidateEntityPermission(typeof(TEntity));
			}
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT выполниться также медленно, как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		protected abstract IQueryOver<TEntity> ItemsQuery(IUnitOfWork uow);

		/// <summary>
		/// Кастомная функция подсчёта кол-ва элементов
		/// </summary>
		protected virtual Func<IUnitOfWork, int> ItemsCountFunction { get; }

		#region Видимость предопределенных действий

		public virtual bool VisibleCreateAction { get; set; } = true;
		public virtual bool VisibleEditAction { get; set; } = true;
		public virtual bool VisibleDeleteAction { get; set; } = true;

		#endregion

		#region Предопределенные действия
		protected virtual void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
		}

		protected virtual void EditEntityDialog(TNode node)
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
		}

		protected virtual void DeleteEntities(TNode[] nodes)
		{
			foreach(var node in nodes)
				DeleteEntityService.DeleteEntity<TEntity>(DomainHelper.GetId(node));
		}
		#endregion
	}
}
