using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;

namespace QSOrmProject.DomainMapping
{
	public class TableView<TEntity> : ITableView
	{
		OrmObjectMapping<TEntity> myObjectMapping;

		private readonly Dictionary<string, Expression<Func<TEntity, string>>> ColumnsFields = new Dictionary<string, Expression<Func<TEntity, string>>>();
		private readonly List<Expression<Func<TEntity, string>>> searchFields = new List<Expression<Func<TEntity, string>>>();
		private readonly List<OrderByItem> OrderFields = new List<OrderByItem> ();

		private FluentColumnsConfig<TEntity> customColumnsConfig;
		private RecursiveTreeConfig<TEntity> treeConfig;

		public TableView (OrmObjectMapping<TEntity> objectMapping)
		{
			myObjectMapping = objectMapping;
		}

		#region ITableView implementation

		GenericSearchProvider<TEntity> searchProvider;

		public ISearchProvider SearchProvider
		{
			get
			{
				if (searchFields.Count == 0)
					return null;

				if (searchProvider != null)
					return searchProvider;

				searchProvider = new GenericSearchProvider<TEntity>();
				searchFields.ForEach(exp => searchProvider.AddSearchByFunc(exp.Compile()));
				return searchProvider;
			}
		}

		public List<OrderByItem> OrderBy {
			get { return OrderFields;
			}
		}

		public IRecursiveTreeConfig RecursiveTreeConfig
		{
			get
			{
				return treeConfig;
			}
		}

		public IColumnsConfig GetGammaColumnsConfig()
		{
			if (customColumnsConfig != null)
				return customColumnsConfig;

			var config = FluentColumnsConfig <TEntity>.Create();
			foreach(var pair in ColumnsFields)
			{
				if(searchFields.Contains(pair.Value))
					config.AddColumn(pair.Key).AddTextRenderer(pair.Value).SearchHighlight();
				else
					config.AddColumn(pair.Key).AddTextRenderer(pair.Value);
			}

			if (typeof(ISpecialRowsRender).IsAssignableFrom(typeof(TEntity)))
			{
				config.RowCells().AddSetter<Gtk.CellRendererText>((c, n) => c.Foreground = (n as ISpecialRowsRender).TextColor);
			}

			return config.Finish();
		}

		#endregion

		#region Config

		public TableView<TEntity> Column(string name, Expression<Func<TEntity, string>> columnFuncExpr)
		{
			ColumnsFields.Add (name, columnFuncExpr);
			return this;
		}

		public TableView<TEntity> SearchColumn(string name, Expression<Func<TEntity, string>> columnFuncExpr)
		{
			ColumnsFields.Add (name, columnFuncExpr);
			searchFields.Add (columnFuncExpr);
			return this;
		}

		public TableView<TEntity> Search(Expression<Func<TEntity, string>> columnFuncExpr)
		{
			searchFields.Add (columnFuncExpr);
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

		public TableView<TEntity> CustomColumnsConfig(FluentColumnsConfig<TEntity> config)
		{
			customColumnsConfig = config;
			return this;
		}

		public TableView<TEntity> TreeConfig(RecursiveTreeConfig<TEntity> config)
		{
			treeConfig = config;
			return this;
		}

		public OrmObjectMapping<TEntity> End()
		{
			return myObjectMapping;
		}

		#endregion 
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

