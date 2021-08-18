using System;
using System.Linq;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class RowMapping<TNode>
	{
		FluentColumnsConfig<TNode> myConfig;

		public RowMapping (FluentColumnsConfig<TNode> parentConfig)
		{
			this.myConfig = parentConfig;
		}

		public RowMapping<TNode> AddSetter<TCellRenderer>(Action<TCellRenderer, TNode> setter) where TCellRenderer : class
		{
			foreach(var cell in myConfig.ConfiguredColumns.OfType<ColumnMapping<TNode>> ()
				.SelectMany (c => c.ConfiguredRenderersGeneric))
			{
				cell.SetCommonSetter<TCellRenderer> (setter);
			}
			return this;
		}

		public RowMapping<TNode> XAlign(float align)
		{
			foreach(var cell in myConfig.ConfiguredColumns
			        .SelectMany(c => c.ConfiguredRenderers)) {
				(cell.GetRenderer() as CellRenderer).Xalign = align;
			}
			return this;
		}

		public IColumnsConfig Finish()
		{
			return myConfig.Finish ();
		}
	}
}

