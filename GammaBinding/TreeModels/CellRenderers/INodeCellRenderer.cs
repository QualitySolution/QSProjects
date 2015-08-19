using System;

namespace GammaBinding.Gtk.Cells
{

	public interface INodeCellRenderer
	{
		void RenderNode(object node);
		string DataPropertyName { get;}
	}

}
