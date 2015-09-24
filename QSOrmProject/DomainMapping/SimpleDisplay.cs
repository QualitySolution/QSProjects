using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QSOrmProject.DomainMapping
{
	public class SimpleDisplay<TEntity>
	{
		OrmObjectMapping<TEntity> myObjectMapping;

		public readonly Dictionary<string, string> ColumnsFields = new Dictionary<string, string>();
		public readonly List<string> SearchFields = new List<string>();

		public SimpleDisplay (OrmObjectMapping<TEntity> objectMapping)
		{
			myObjectMapping = objectMapping;
		}

		public SimpleDisplay<TEntity> Column(string name, Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			ColumnsFields.Add (name, propName);
			return this;
		}

		public SimpleDisplay<TEntity> SearchColumn(string name, Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			ColumnsFields.Add (name, propName);
			SearchFields.Add (propName);
			return this;
		}

		public SimpleDisplay<TEntity> Search(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propName = PropertyUtil.GetName (propertyRefExpr);
			SearchFields.Add (propName);
			return this;
		}

		public OrmObjectMapping<TEntity> End()
		{
			return myObjectMapping;
		}
	}
}

