using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tools;
using System.Linq;
using System.Reflection;

namespace QS.RepresentationModel.GtkUI
{
	internal class EntityCommonRepresentationModel<TEntity> : RepresentationModelBase<TEntity>, IRepresentationModel
		where TEntity : class, IDomainObject, new()
	{
		internal ICriterion FixedRestriction { get; set; }
		internal IEnumerable<OrderByField<TEntity>> Orders { get; set; }

		private IColumnsConfig columnsConfig;

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		public Type EntityType => typeof(TEntity);

		private IJournalFilter journalFilter;
		public IJournalFilter JournalFilter {
			get { return journalFilter; }
			set {
				journalFilter = value;
				if(journalFilter != null) {
					journalFilter.Refiltered -= JournalFilter_Refiltered;
					journalFilter.Refiltered += JournalFilter_Refiltered;
				}
			}
		}

		void JournalFilter_Refiltered(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		public IQueryFilter QueryFilter { get; set; }

		public EntityCommonRepresentationModel(IUnitOfWork uow, IColumnsConfig columnsConfig)
		{
			UoW = uow;
			this.columnsConfig = columnsConfig;
		}

		public override void UpdateNodes()
		{
			if(UoW != null) {
				SetItemsSource(GetItems(UoW));
			} else {
				using(var localUoW = UnitOfWorkFactory.CreateWithoutRoot()) {
					SetItemsSource(GetItems(localUoW));
				}
			}
		}

		private IList<TEntity> GetItems(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<TEntity>();

			if(FixedRestriction != null) {
				query.Where(FixedRestriction);
			}

			var filtration = QueryFilter?.GetFilter();
			if(filtration != null) {
				query.Where(filtration);
			}

			foreach(var order in Orders) {
				if(order.IsDesc) {
					query = query.OrderBy(order.orderExpr).Desc;
				} else {
					query = query.OrderBy(order.orderExpr).Asc;
				}
			}

			return query.List();
		}

		internal List<Expression<Func<TEntity, object>>> EntitySearchFields { get; set; }

		public override IEnumerable<string> SearchFields {
			get {
				return EntitySearchFields.Select(x => PropertyUtil.GetName(x)).Where(x => !string.IsNullOrWhiteSpace(x));
			}
		}

		protected override PropertyInfo[] SearchPropCache {
			get {
				return EntitySearchFields.Select(PropertyUtil.GetPropertyInfo).ToArray();
			}
		}

		public virtual IEnumerable<IJournalPopupItem> PopupItems => new List<IJournalPopupItem>();

		public void Destroy()
		{
		}
	}
}
