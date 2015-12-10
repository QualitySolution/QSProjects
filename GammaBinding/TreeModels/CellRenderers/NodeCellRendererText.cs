using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using Gamma.Binding;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererText<TNode> : CellRendererText, INodeCellRenderer
	{
		public List<Action<NodeCellRendererText<TNode>, TNode>> LambdaSetters;

		public string DataPropertyName { get{ return DataPropertyInfo.Name;
			}}

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}

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

