﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Gamma.Binding;
using Gtk;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererSpin<TNode> : CellRendererSpin, INodeCellRenderer, INodeCellRendererCanGoNextCell
	{
		public List<Action<NodeCellRendererSpin<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererSpin<TNode>, TNode>>();

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}

		public NodeCellRendererSpin ()
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
			
		public bool IsEnterToNextCell { get; set;}
	}
}

