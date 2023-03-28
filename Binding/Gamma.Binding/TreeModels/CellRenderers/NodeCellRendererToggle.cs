﻿using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using Gamma.Binding;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererToggle<TNode> : CellRendererToggle, INodeCellRenderer
	{
		public List<Action<NodeCellRendererToggle<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererToggle<TNode>, TNode>>();

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}

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

