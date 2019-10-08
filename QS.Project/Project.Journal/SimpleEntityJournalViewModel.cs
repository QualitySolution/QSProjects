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

namespace QS.Project.Journal
{
	public class SimpleEntityJournalViewModel<TEntity, TEntityTab> : SimpleEntityJournalViewModelBase, IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TEntityTab : class, ITdiTab
	{
		public bool IsActive => UoW.IsAlive;

		public SimpleEntityJournalViewModel(
			Expression<Func<TEntity, object>> titleExp,
			Func<TEntityTab> createDlgFunc,
			Func<CommonJournalNode, TEntityTab> openDlgFunc,
			ICommonServices commonServices) : base(typeof(TEntity), commonServices)
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

		protected Func<IQueryOver<TEntity>> ItemsSourceQueryFunction => () => {
			var query = UoW.Session.QueryOver<TEntity>();
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
