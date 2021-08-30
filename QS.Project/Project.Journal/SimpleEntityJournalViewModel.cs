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
using QS.Project.Journal.Actions.ViewModels;

namespace QS.Project.Journal
{
	public class SimpleEntityJournalViewModel<TEntity, TEntityTab> : SimpleEntityJournalViewModelBase, IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TEntityTab : class, ITdiTab
	{
		public SimpleEntityJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			Expression<Func<TEntity, object>> titleExp,
			Func<TEntityTab> createDlgFunc,
			Func<JournalEntityNodeBase, TEntityTab> openDlgFunc,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(journalActionsViewModel, typeof(TEntity), unitOfWorkFactory, commonServices)
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

			InitializeJournalActionsViewModel();
		}

		protected override void InitializeJournalActionsViewModel()
		{
			EntitiesJournalActionsViewModel.Initialize(
				EntityConfigs, this, HideJournal, true, addActionEnabled, editActionEnabled, deleteActionEnabled);
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
