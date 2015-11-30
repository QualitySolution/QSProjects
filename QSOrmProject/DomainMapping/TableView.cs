using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QSOrmProject.DomainMapping
{
	public class TableView<TEntity> : ITableView
	{
		OrmObjectMapping<TEntity> myObjectMapping;

		public readonly Dictionary<string, string> ColumnsFields = new Dictionary<string, string>();
		private readonly List<string> SearchFields = new List<string>();
		private readonly List<OrderByItem> OrderFields = new List<OrderByItem> ();

		public TableView (OrmObjectMapping<TEntity> objectMapping)
		{
			myObjectMapping = objectMapping;
		}

		#region ITableView implementation

		public List<OrderByItem> OrderBy {
			get { return OrderFields;
			}
		}

		public List<string> SearchBy {
			get { return SearchFields;
			}
		}

		#endregion

		public TableView<TEntity> Column(string name, Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			ColumnsFields.Add (name, propName);
			return this;
		}

		public TableView<TEntity> SearchColumn(string name, Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			ColumnsFields.Add (name, propName);
			SearchFields.Add (propName);
			return this;
		}

		public TableView<TEntity> Search(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			SearchFields.Add (propName);
			return this;
		}

		public TableView<TEntity> OrderAsc(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			OrderFields.Add (new OrderByItem(propName, OrderDirection.Asc));
			return this;
		}

		public TableView<TEntity> OrderDesc(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			OrderFields.Add (new OrderByItem(propName, OrderDirection.Desc));
			return this;
		}

		public OrmObjectMapping<TEntity> End()
		{
			return myObjectMapping;
		}
	}

	public enum OrderDirection
	{
		Asc,
		Desc
	}

	public class OrderByItem
	{
		public string PropertyName { get; set;}
		public OrderDirection Direction { get; set;}

		public OrderByItem(string propertyName, OrderDirection direction)
		{
			PropertyName = propertyName;
			Direction = direction;
		}
	}
}

