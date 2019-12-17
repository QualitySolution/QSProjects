using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class EntityJournalViewModelBase<TEntity, TEntityViewModel, TNode> : JournalViewModelBase
		where TEntity : class, IDomainObject
		where TEntityViewModel : ViewModelBase
		where TNode : class
	{
		#region Обязательные зависимости
		#endregion
		#region Опциональные зависимости
		protected INavigationManager NavigationManager; //Необязательный так как передопределнные методы открытия диалогов могут быть заменены на неиспользуемые менеджер навигации.
		protected IDeleteEntityService DeleteEntityService; //Опционально аналогично предыдущиему сервису.
		public IPermissionService CurrentPermissionService { get; set; }
		#endregion

		protected EntityJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager = null,
			IDeleteEntityService deleteEntityService = null,
			IPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService)
		{
			NavigationManager = navigationManager;
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;

			var dataLoader = new ThreadDataLoader<TNode>(unitOfWorkFactory);
			dataLoader.PermissionService = currentPermissionService;
			dataLoader.AddQuery<TEntity>(ItemsQuery);
			DataLoader = dataLoader;

			if(currentPermissionService != null && !currentPermissionService.ValidateEntityPermissionForCurrentUser(typeof(TEntity)).CanRead)
				throw new AbortCreatingPageException($"У вас нет прав для просмотра документов типа: {typeof(TEntity).GetSubjectName()}", "Невозможно открыть журнал");

			CreateNodeActions();

			var names = typeof(TEntity).GetSubjectNames();
			if(!String.IsNullOrEmpty(names?.NominativePlural))
				TabName = names.NominativePlural.StringToTitleCase();

			//Поиск
			Search.OnSearch += Search_OnSearch;
			searchHelper = new SearchHelper(Search);

			UpdateOnChanges(typeof(TEntity));
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			bool canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermissionForCurrentUser(typeof(TEntity)).CanCreate;
			bool canEdit = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermissionForCurrentUser(typeof(TEntity)).CanUpdate;
			bool canDelete = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermissionForCurrentUser(typeof(TEntity)).CanDelete;

			var addAction = new JournalAction("Добавить",
					(selected) => canCreate,
					(selected) => true,
					(selected) => CreateEntityDialog()
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => canEdit,
					(selected) => true,
					(selected) => selected.Cast<TNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
					(selected) => canDelete,
					(selected) => true,
					(selected) => DeleteEntities(selected.Cast<TNode>().ToArray())
					);
			NodeActionsList.Add(deleteAction);
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT будет выполнятся также медленно как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		protected abstract IQueryOver<TEntity> ItemsQuery(IUnitOfWork uow);

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

		#region Поиск

		void Search_OnSearch(object sender, EventArgs e)
		{
			Refresh();
		}

		private readonly SearchHelper searchHelper;

		protected ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			return searchHelper.GetSearchCriterion(aliasPropertiesExpr);
		}

		protected ICriterion GetSearchCriterion<TRootEntity>(params Expression<Func<TRootEntity, object>>[] propertiesExpr)
		{
			return searchHelper.GetSearchCriterion(propertiesExpr);
		}

		#endregion
	}
}
