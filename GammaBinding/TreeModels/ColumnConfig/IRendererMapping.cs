using System;
using GammaBinding.Gtk.Cells;

namespace GammaBinding.ColumnConfig
{
	public interface IRendererMapping
	{
		INodeCellRenderer GetRenderer();
		bool IsExpand { get;}
	}

	public interface IRendererMappingGeneric<TNode> : IRendererMapping
	{
		void SetCommonSetter<TCellRenderer>(Action<TCellRenderer, TNode> commonSet) where TCellRenderer : class;
	}
}

