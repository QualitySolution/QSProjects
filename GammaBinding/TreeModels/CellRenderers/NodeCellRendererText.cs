using System;
using System.Collections.Generic;
using Gtk;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererText<TNode> : CellRendererText, INodeCellRenderer
	{
		public List<Action<NodeCellRendererText<TNode>, TNode>> LambdaSetters;

		public string DataPropertyName { get; set;}

		public NodeCellRendererText ()
		{
		}

		public void RenderNode(object node)
		{
			
			if(node is TNode)
			{
				var typpedNode = (TNode)node;
				LambdaSetters.ForEach (a => a.Invoke (this, typpedNode));
			}
		}

	}
}

