using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel.GtkUI
{
	public class EntityCommonRepresentationModelConstructor<TEntity>
		where TEntity : class, IDomainObject, new()
	{
		private readonly Dictionary<string, Expression<Func<TEntity, string>>> columnsFields = new Dictionary<string, Expression<Func<TEntity, string>>>();
		private readonly List<Expression<Func<TEntity, string>>> searchFields = new List<Expression<Func<TEntity, string>>>();
		private readonly List<Expression<Func<TEntity, bool>>> filtersFields = new List<Expression<Func<TEntity, bool>>>();
		private readonly List<OrderByField<TEntity>> ordersFields = new List<OrderByField<TEntity>>();

		private IColumnsConfig ColumnsConfig;

		public EntityCommonRepresentationModelConstructor<TEntity> AddColumn(string name, Expression<Func<TEntity, string>> columnFuncExpr)
		{
			columnsFields.Add(name, columnFuncExpr);
			return this;
		}

		public EntityCommonRepresentationModelConstructor<TEntity> AddSearchColumn(string name, Expression<Func<TEntity, string>> columnFuncExpr)
		{
			columnsFields.Add(name, columnFuncExpr);
			searchFields.Add(columnFuncExpr);
			return this;
		}

		public EntityCommonRepresentationModelConstructor<TEntity> AddFilter(Expression<Func<TEntity, bool>> filterFuncExpr)
		{
			filtersFields.Add(filterFuncExpr);
			return this;
		}

		public EntityCommonRepresentationModelConstructor<TEntity> OrderBy(Expression<Func<TEntity, object>> orderFuncExpr)
		{
			ordersFields.Add(new OrderByField<TEntity>(orderFuncExpr, false));
			return this;
		}

		public EntityCommonRepresentationModelConstructor<TEntity> OrderByDesc(Expression<Func<TEntity, object>> orderFuncExpr)
		{
			ordersFields.Add(new OrderByField<TEntity>(orderFuncExpr, true));
			return this;
		}

		private IColumnsConfig GetGammaColumnsConfig()
		{
			var config = FluentColumnsConfig<TEntity>.Create();
			foreach(var pair in columnsFields) {
				if(searchFields.Contains(pair.Value))
					config.AddColumn(pair.Key).AddTextRenderer(pair.Value).SearchHighlight();
				else
					config.AddColumn(pair.Key).AddTextRenderer(pair.Value);
			}
			return config.Finish();
		}

		public IRepresentationModel Finish()
		{
			var resultVM = new EntityCommonRepresentationModel<TEntity>(GetGammaColumnsConfig());
			resultVM.filters = filtersFields;
			resultVM.orders = ordersFields;
			return resultVM;
		}
	}

	public class OrderByField<TEntity>
		where TEntity : class, IDomainObject, new()
	{
		public Expression<Func<TEntity, object>> orderExpr { get; private set; }
		public bool IsDesc { get; private set; }

		public OrderByField(Expression<Func<TEntity, object>> orderExpr, bool desc)
		{
			this.orderExpr = orderExpr;
			IsDesc = desc;
		}
	}
}
