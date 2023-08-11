using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class FluentColumnsConfig<TNode> : IColumnsConfig
	{
		List<ColumnMapping<TNode>> Columns = new List<ColumnMapping<TNode>>();

		RowMapping<TNode> row;

		yTreeView myTreeView;

		public IEnumerable<IColumnMapping> ConfiguredColumns {
			get { return Columns.OfType<IColumnMapping> ();	}
		}

		public Func<IyTreeModel> TreeModelFunc { get; private set; }

		public FluentColumnsConfig ()
		{
		}

		internal FluentColumnsConfig(yTreeView treeview)
		{
			myTreeView = treeview;
		}

		public static FluentColumnsConfig<TNode> Create()
		{
			return new FluentColumnsConfig<TNode> ();
		}

		public FluentColumnsConfig<TNode> SetTreeModel(Func<IyTreeModel> treeModel) {
			TreeModelFunc = treeModel;
			return this;
		}

		public ColumnMapping<TNode> AddColumn(string title)
		{
			var column = new ColumnMapping<TNode> (this, title);
			Columns.Add (column);
			return column;
		}

		public RowMapping<TNode> RowCells()
		{
			row = new RowMapping<TNode> (this);
			return row;
		}

		public IColumnsConfig Finish()
		{
			if(myTreeView != null)
				myTreeView.ColumnsConfig = this;
			return this;
		}

		public IEnumerable<IRendererMapping> GetRendererMappingByTag(object tag)
		{
			return Columns.SelectMany(x => x.ConfiguredRenderersGeneric).Where(x => TypeUtil.EqualBoxedValues(tag, x.tag));
		}

		public IEnumerable<TreeViewColumn> GetColumnsByTag(object tag)
		{
			return Columns.Where(x => TypeUtil.EqualBoxedValues(tag, x.tag)).Select(x => x.TreeViewColumn);
		}

		public IEnumerable<TRendererMapping> GetRendererMappingByTagGeneric<TRendererMapping>(object tag)
		{
			return Columns.SelectMany(x => x.ConfiguredRenderersGeneric).Where(x => TypeUtil.EqualBoxedValues(tag, x.tag)).OfType<TRendererMapping>();
		}
	}
}

