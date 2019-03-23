using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tools;

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

		public EntityCommonRepresentationModel(IColumnsConfig columnsConfig)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.columnsConfig = columnsConfig;
		}

		public override void UpdateNodes()
		{
			var query = UoW.Session.QueryOver<TEntity>();

			if(FixedRestriction != null) {
				query.Where(FixedRestriction);
			}

			if(QueryFilter != null) {
				query.Where(QueryFilter.GetFilter());
			}

			foreach(var order in Orders) {
				if(order.IsDesc) {
					query = query.OrderBy(order.orderExpr).Desc;
				} else {
					query = query.OrderBy(order.orderExpr).Asc;
				}
			}

			var itemslist = query.List();
			SetItemsSource(itemslist);
		}

		public void Destroy()
		{
			UoW.Dispose();
		}
	}
}
