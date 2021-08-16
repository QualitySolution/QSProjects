using System;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;
using Gamma.Binding;

namespace Gamma.ColumnConfig
{
	public abstract class RendererMappingBase<TCellRenderer, TNode> : IRendererMappingGeneric<TNode>
	{
		ColumnMapping<TNode> myColumn;

		public bool IsExpand { get; set;}

		public object tag { get; set; }

		protected RendererMappingBase (ColumnMapping<TNode> parentColumn)
		{
			myColumn = parentColumn;
		}
			
		public abstract INodeCellRenderer GetRenderer ();

		protected abstract void SetSetterSilent(Action<TCellRenderer, TNode> commonSet);

		public void SetCommonSetter<TActionCellRenderer>(Action<TActionCellRenderer, TNode> commonSet) where TActionCellRenderer : class
		{
			if(typeof(TActionCellRenderer).IsAssignableFrom (typeof(TCellRenderer)))
			{
				SetSetterSilent ((c, n) => commonSet(c as TActionCellRenderer, n));
			}
		}

		public ColumnMapping<TNode> AddColumn(string title)
		{
			return myColumn.AddColumn (title);
		}

		public RowMapping<TNode> RowCells()
		{
			return myColumn.RowCells ();
		}

		public IColumnsConfig Finish()
		{
			return myColumn.Finish ();
		}

		#region Renderers

		public TextRendererMapping<TNode> AddTextRenderer(Expression<Func<TNode, string>> dataProperty, bool expand = true, bool useMarkup = false)
		{
			return myColumn.AddTextRenderer (dataProperty, expand, useMarkup);
		}

		public TextRendererMapping<TNode> AddTextRenderer()
		{
			return myColumn.AddTextRenderer ();
		}

		public ProgressRendererMapping<TNode> AddProgressRenderer(Expression<Func<TNode, int>> dataProperty, bool expand = true)
		{
			return myColumn.AddProgressRenderer (dataProperty, expand);
		}

		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			return myColumn.AddNumericRenderer (dataProperty, expand);
		}

		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, IValueConverter converter, bool expand = true)
		{
			return myColumn.AddNumericRenderer (dataProperty, converter, expand);
		}

		public EnumRendererMapping<TNode, TItem> AddEnumRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true, Enum [] excludeItems = null) where TItem : struct, IConvertible
		{
			return myColumn.AddEnumRenderer (dataProperty, expand, excludeItems);
		}

		public ComboRendererMapping<TNode, TItem> AddComboRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true)
		{
			return myColumn.AddComboRenderer (dataProperty, expand);
		}

		public ToggleRendererMapping<TNode> AddToggleRenderer(Expression<Func<TNode, bool>> dataProperty, bool expand = true)
		{
			return myColumn.AddToggleRenderer (dataProperty, expand);
		}

		public PixbufRendererMapping<TNode> AddPixbufRenderer (Expression<Func<TNode, Gdk.Pixbuf>> dataProperty, bool expand = true)
		{
			return myColumn.AddPixbufRenderer (dataProperty, expand);
		}

		#endregion
	}
}

