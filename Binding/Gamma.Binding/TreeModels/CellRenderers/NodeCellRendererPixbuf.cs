using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using Gamma.Binding;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererPixbuf<TNode> : CellRendererPixbuf, INodeCellRenderer
	{
		public List<Action<NodeCellRendererPixbuf<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererPixbuf<TNode>, TNode>>();

		public string DataPropertyName { get{ return DataPropertyInfo.Name;
			}}

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}

		public NodeCellRendererPixbuf ()
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

