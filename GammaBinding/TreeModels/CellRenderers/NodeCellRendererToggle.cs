using System;
using System.Collections.Generic;
using Gtk;

namespace GammaBinding.Gtk.Cells
{
	public class NodeCellRendererToggle<TNode> : CellRendererToggle, INodeCellRenderer
	{
		public List<Action<NodeCellRendererToggle<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererToggle<TNode>, TNode>>();

		public string DataPropertyName { get; set;}

		public NodeCellRendererToggle ()
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

