using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Services;
using QS.Tdi;
using NHibernate.Transform;
using System.Linq.Expressions;
using NHibernate.Criterion;
using QS.Project.Journal.EntitySelector;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;

namespace QS.Project.Journal
{
	public class SimpleEntityJournalViewModel<TEntity, TEntityTab> : SimpleEntityJournalViewModelBase, IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TEntityTab : class, ITdiTab
		//where TSearchModel : CriterionSearchModelBase
	{
		public SimpleEntityJournalViewModel(
			Expression<Func<TEntity, object>> titleExp,
			Func<TEntityTab> createDlgFunc,
			Func<CommonJournalNode, TEntityTab> openDlgFunc,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			SearchViewModelBase<CriterionSearchModel> searchViewModel) : base(typeof(TEntity), unitOfWorkFactory, commonServices, searchViewModel)
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

			query.Where(CriterionSearchModel.ConfigureSearch()
				.AddSearchBy(titleExp)
				.GetSearchCriterion()
			);

			return query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(titleExp).WithAlias(() => resultAlias.Title)
				)
				.TransformUsing(Transformers.AliasToBean<CommonJournalNode<TEntity>>());
		};

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
	}
}
