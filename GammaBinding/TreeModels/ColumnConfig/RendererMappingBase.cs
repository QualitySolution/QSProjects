using System;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;

namespace Gamma.ColumnConfig
{
	public abstract class RendererMappingBase<TCellRenderer, TNode> : IRendererMappingGeneric<TNode>
	{
		ColumnMapping<TNode> myColumn;

		public bool IsExpand { get; set;}

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

		public TextRendererMapping<TNode> AddTextRenderer(Expression<Func<TNode, string>> dataProperty, bool expand = true)
		{
			return myColumn.AddTextRenderer (dataProperty, expand);
		}

		public TextRendererMapping<TNode> AddTextRenderer()
		{
			return myColumn.AddTextRenderer ();
		}

		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			return myColumn.AddNumericRenderer (dataProperty, expand);
		}

		public EnumRendererMapping<TNode> AddEnumRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			return myColumn.AddEnumRenderer (dataProperty, expand);
		}

		public ComboRendererMapping<TNode> AddComboRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			return myColumn.AddComboRenderer (dataProperty, expand);
		}

		public ToggleRendererMapping<TNode> AddToggleRenderer(Expression<Func<TNode, bool>> dataProperty, bool expand = true)
		{
			return myColumn.AddToggleRenderer (dataProperty, expand);
		}

		#endregion
	}
}

