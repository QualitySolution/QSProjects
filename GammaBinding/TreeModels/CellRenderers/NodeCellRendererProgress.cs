﻿using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererProgress<TNode> : CellRendererProgress, INodeCellRenderer
	{
		public List<Action<NodeCellRendererProgress<TNode>, TNode>> LambdaSetters;

		public string DataPropertyName { get{ return DataPropertyInfo.Name;
			}}

		public PropertyInfo DataPropertyInfo { get; set;}

		public NodeCellRendererProgress ()
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

