using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel.GtkUI
{
	internal class EntityCommonRepresentationModel<TEntity> : RepresentationModelBase<TEntity>, IRepresentationModel
		where TEntity : class, IDomainObject, new()
	{
		internal IEnumerable<Expression<Func<TEntity, bool>>> filters { get; set; }
		internal IEnumerable<OrderByField<TEntity>> orders { get; set; }

		private IColumnsConfig columnsConfig;
		public override IColumnsConfig ColumnsConfig => columnsConfig;

		public Type EntityType => typeof(TEntity);

		public IJournalFilter JournalFilter => null;

		public EntityCommonRepresentationModel(IColumnsConfig columnsConfig)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.columnsConfig = columnsConfig;
		}

		public override void UpdateNodes()
		{
			var query = UoW.Session.QueryOver<TEntity>();
			foreach(var filter in filters) {
				query.Where(filter);
			}
			foreach(var order in orders) {
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
