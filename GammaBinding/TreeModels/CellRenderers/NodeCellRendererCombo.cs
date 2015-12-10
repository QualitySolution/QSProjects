using System;
using System.Collections.Generic;
using System.Reflection;
using Gtk;
using Gamma.Binding;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererCombo<TNode> : CellRendererCombo, INodeCellRenderer
	{
		public List<Action<NodeCellRendererCombo<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererCombo<TNode>, TNode>>();

		public string DataPropertyName { get{ return DataPropertyInfo.Name;
			}}

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}

		public Func<object, string> DisplayFunc { get; set;}

		public NodeCellRendererCombo ()
		{
			HasEntry = false;
		}

		public void RenderNode(object node)
		{
			if(node is TNode)
			{
				var propValue = DataPropertyInfo.GetValue (node, null);
				if (propValue != null)
					Text = DisplayFunc == null ? propValue.ToString () : DisplayFunc (propValue);
				else
					Text = String.Empty;

				var typpedNode = (TNode)node;
				LambdaSetters.ForEach (a => a.Invoke (this, typpedNode));
			}
		}
	}

	public enum NodeCellRendererColumns
	{
		value,
		title
	}
}

