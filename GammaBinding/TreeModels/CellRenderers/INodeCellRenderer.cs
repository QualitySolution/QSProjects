using System;

namespace Gamma.GtkWidgets.Cells
{

	public interface INodeCellRenderer
	{
		void RenderNode(object node);
		string DataPropertyName { get;}
	}

}
