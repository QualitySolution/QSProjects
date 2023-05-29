using System;
using System.ComponentModel;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Services;
using QS.Tdi;
using NHibernate.Transform;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Criterion;
using QS.Project.Journal.EntitySelector;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;

namespace QS.Project.Journal
{
	public class SimpleEntityJournalViewModel<TEntity, TEntityTab> : SimpleEntityJournalViewModelBase, IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TEntityTab : class, ITdiTab
	{
		public SimpleEntityJournalViewModel(
			Expression<Func<TEntity, object>> titleExp,
			Func<TEntityTab> createDlgFunc,
			Func<CommonJournalNode, TEntityTab> openDlgFunc,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(typeof(TEntity), unitOfWorkFactory, commonServices)
		{
			this.titleExp = titleExp ?? throw new ArgumentNullException(nameof(titleExp));

			if(createDlgFunc == null) {
				throw new ArgumentNullException(nameof(createDlgFunc));
			}

			if(openDlgFunc == null) {
				throw new ArgumentNullException(nameof(openDlgFunc));
			}

			Register<TEntity, TEntityTab>(ItemsSourceQueryFunction, createDlgFunc, openDlgFunc);
			ExternalNotifyChangedWith(typeof(TEntity));

			DataLoader.ItemsListUpdated += (sender, e) => ListUpdated?.Invoke(sender, e);
		}

		#region Управление стандартными кнопками

		protected bool addActionEnabled = true;
		protected bool editActionEnabled = true;
		protected bool deleteActionEnabled = true;

		public void SetActionsVisible(bool addActionEnabled = true, bool editActionEnabled = true, bool deleteActionEnabled = true)
		{
			this.addActionEnabled = addActionEnabled;
			this.editActionEnabled = editActionEnabled;
			this.deleteActionEnabled = deleteActionEnabled;

			CreateNodeActions();
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			if(addActionEnabled)
				CreateDefaultAddActions();
			if(editActionEnabled)
				CreateDefaultEditAction();
			if(deleteActionEnabled)
				CreateDefaultDeleteAction();
		}

		#endregion

		public void ExternalNotifyChangedWith(params Type[] entityTypes)
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnExternalUpdate, entityTypes);
		}

		void OnExternalUpdate(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		CommonJournalNode<TEntity> resultAlias = null;

		private Func<ICriterion> filterFunction;
		private Func<ICriterion> restrictionFunc;
		private readonly Expression<Func<TEntity, object>> titleExp;

		public event EventHandler ListUpdated;

		public void SetFilter<TFilter>(TFilter filter, Func<TFilter, ICriterion> filterFunc)
			where TFilter : class, IJournalFilter
		{
			Filter = filter;
			filterFunction = () => filterFunc.Invoke(filter);
		}

		public void SetRestriction(Func<ICriterion> restrictionFunc)
		{
			this.restrictionFunc = restrictionFunc;
		}

		public void SearchValues(params string[] values)
		{
			Search.SearchValues = values;
		}

		protected Func<IUnitOfWork, IQueryOver<TEntity>> ItemsSourceQueryFunction => (uow) => {
			var query = uow.Session.QueryOver<TEntity>();
			if(filterFunction != null) {
				query.Where(filterFunction.Invoke());
			}
			if(restrictionFunc != null) {
				query.Where(restrictionFunc.Invoke());
			}

			query.Where(GetSearchCriterion(
				titleExp
			));

			return query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(titleExp).WithAlias(() => resultAlias.DisplayName)
				)
				.TransformUsing(Transformers.AliasToBean<CommonJournalNode<TEntity>>());
		};

		public override ITdiTab GetTabToOpen(Type subjectType, int subjectId)
		{
			var constructorInfos = typeof(CommonJournalNode<TEntity>)
				.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			var node = constructorInfos.First().Invoke(new object[]{}) as CommonJournalNode;
			if (node == null)
				return null;

			node.Id = subjectId;
			EntityConfigs.TryGetValue(subjectType, out var config);
			var foundDocumentConfig =
				(config ?? throw new InvalidOperationException($"Не найдена конфигурация для {subjectType.Name}"))
				.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(node));

			return (foundDocumentConfig?.GetOpenEntityDlgFunction()
			        ?? throw new InvalidOperationException(
				        $"Не найдена конфигурация для открытия диалога в {nameof(foundDocumentConfig)}"))
				.Invoke(node);
		}
	}

	public class CommonJournalNode<TEntity> : CommonJournalNode
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
	{
		protected CommonJournalNode() : base(typeof(TEntity))
		{
		}
	}

	public class CommonJournalNode : JournalEntityNodeBase
	{


		protected CommonJournalNode(Type entityType) : base(entityType)
		{
		}

		public string DisplayName { get; set ; }

		public override string Title => DisplayName;
	}
}
