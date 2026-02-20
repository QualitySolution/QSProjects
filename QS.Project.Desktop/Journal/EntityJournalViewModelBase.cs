using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Journal.Actions;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.NodeExtensions;
using QS.Project.Services;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace QS.Journal
{
	public abstract class EntityJournalViewModelBase<TEntity, TEntityViewModel, TNode> : UowJournalViewModelBase<TNode>
		where TEntity : class, IDomainObject
		where TEntityViewModel : DialogViewModelBase
		where TNode : class
	{
		#region Опциональные зависимости
		protected IDeleteEntityService DeleteEntityService;
		public ICurrentPermissionService CurrentPermissionService { get; set; }
		#endregion

		#region Предопределенные действия
		/// <summary>
		/// Действие "Выбрать" для режима выбора
		/// </summary>
		protected JournalAction<TNode> SelectAction { get; set; }
		
		/// <summary>
		/// Действие "Изменить/Открыть" для редактирования
		/// </summary>
		protected JournalAction<TNode> EditAction { get; set; }
		#endregion

		protected EntityJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IEntityChangeWatcher changeWatcher,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, navigationManager, changeWatcher)
		{
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;

			var dataLoader = new ThreadDataLoader<TNode>(unitOfWorkFactory);
			dataLoader.CurrentPermissionService = currentPermissionService;
			dataLoader.AddQuery<TEntity>(ItemsQuery, ItemsCountFunction);
			DataLoader = dataLoader;

			if(currentPermissionService != null && !currentPermissionService.ValidateEntityPermission(typeof(TEntity)).CanRead)
				throw new AbortCreatingPageException($"У вас нет прав для просмотра документов типа: {typeof(TEntity).GetSubjectName()}", "Невозможно открыть журнал");

			// Создаем новую view model для действий
			var actionsViewModel = new ButtonJournalActionsViewModel<TNode>();
			actionsViewModel.Journal = this;
			ActionsViewModel = actionsViewModel;

			CreateNodeActions();
			
			// Подписываемся на изменение SelectionMode для обновления DoubleClickAction
			PropertyChanged += (sender, args) => {
				if(args.PropertyName == nameof(SelectionMode)) {
					UpdateDoubleClickAction();
				}
			};

			var names = typeof(TEntity).GetSubjectNames();
			if(!String.IsNullOrEmpty(names?.NominativePlural))
				Title = names.NominativePlural.StringToTitleCase();

			UpdateOnChanges(typeof(TEntity));
		}

		protected void CreateNodeActions()
		{
			var actionsViewModel = (ButtonJournalActionsViewModel<TNode>)ActionsViewModel;
			
			// Действие "Выбрать" (только для режима выбора)
			SelectAction = new JournalAction<TNode>(
				"Выбрать",
				selected => OnItemsSelected(selected.Cast<object>().ToArray()),
				selected => selected.Any(),
				selected => SelectionMode != JournalSelectionMode.None
			);
			actionsViewModel.AddAction(SelectAction);
			
			bool canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(TEntity)).CanCreate;

			// Действие "Создать"
			var addAction = new JournalAction<TNode>(
				"Создать",
				selected => CreateEntityDialog(),
				selected => canCreate,
				selected => VisibleCreateAction,
				"Insert"
			);
			actionsViewModel.AddAction(addAction);

			// Действие "Изменить/Открыть"
			EditAction = new JournalAction<TNode>(
				selected => {
					var permissions = CalculatePermission(selected.Cast<object>().ToArray());
					return permissions.Any(p => p.CanUpdate) ? "Изменить" : "Открыть";
				},
				selected => selected.ToList().ForEach(EditEntityDialog),
				selected => selected.Any() && CalculatePermission(selected.Cast<object>().ToArray()).All(s => s.CanUpdate || s.CanRead),
				selected => VisibleEditAction
			);
			actionsViewModel.AddAction(EditAction);

			// Действие "Удалить"
			var deleteAction = new JournalAction<TNode>(
				"Удалить",
				selected => DeleteEntities(selected.ToArray()),
				selected => selected.Any() && CalculatePermission(selected.Cast<object>().ToArray()).All(s => s.CanDelete),
				selected => VisibleDeleteAction,
				"Delete"
			);
			actionsViewModel.AddAction(deleteAction);

			// Устанавливаем действие при двойном клике
			UpdateDoubleClickAction();
			
			// Добавляем кнопку "Обновить" справа
			AddRefreshAction(actionsViewModel);
		}

		/// <summary>
		/// Добавляет кнопку "Обновить" в правую часть панели
		/// </summary>
		protected virtual void AddRefreshAction(ButtonJournalActionsViewModel<TNode> actionsViewModel)
		{
			var refreshAction = new JournalAction<TNode>(
				"Обновить",
				selected => Refresh(),
				selected => true,
				selected => true,
				"F5"
			);
			actionsViewModel.AddRightAction(refreshAction);
		}

		/// <summary>
		/// Обновляет действие при двойном клике в зависимости от режима выбора
		/// </summary>
		protected virtual void UpdateDoubleClickAction()
		{
			var actionsViewModel = (ButtonJournalActionsViewModel<TNode>)ActionsViewModel;
			if(actionsViewModel == null)
				return;
				
			// В режиме выбора - выбор элемента, иначе - редактирование
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple) {
				actionsViewModel.DoubleClickAction = SelectAction;
			} else {
				actionsViewModel.DoubleClickAction = EditAction;
			}
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
