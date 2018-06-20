using System;
using System.Collections.Generic;
using System.Reflection;
using Gamma.Binding;
using Gdk;
using Gtk;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererCombo<TNode, TItem> : CellRendererCombo, INodeCellRendererCombo
	{
		public List<Action<NodeCellRendererCombo<TNode, TItem>, TNode>> LambdaSetters = new List<Action<NodeCellRendererCombo<TNode, TItem>, TNode>>();

		public string DataPropertyName {
			get {
				return DataPropertyInfo.Name;
			}
		}

		public PropertyInfo DataPropertyInfo { get; set; }

		public IValueConverter EditingValueConverter { get; set; }

		public Func<TItem, string> DisplayFunc { get; set; }

		public yTreeView MyTreeView { get; set; }

		public Func<TNode, TItem, bool> HideItemFunc { get; set; }

		public bool IsDynamicallyFillList => HideItemFunc != null;

		public Action<TNode> FillComboListFunc;

		public NodeCellRendererCombo()
		{
			HasEntry = false;
		}

		public void RenderNode(object node)
		{
			if(node is TNode) {
				var propValue = (TItem)DataPropertyInfo.GetValue(node, null);
				if(propValue != null)
					Text = DisplayFunc == null ? propValue.ToString() : DisplayFunc(propValue);
				else
					Text = String.Empty;

				var typpedNode = (TNode)node;
				LambdaSetters.ForEach(a => a.Invoke(this, typpedNode));
			}
		}

		public void RefillComboList(object node)
		{
			if(!IsDynamicallyFillList)
				return;

			FillComboListFunc((TNode)node);
		}

		public override CellEditable StartEditing(Event evnt, Widget widget, string path, Rectangle background_area, Rectangle cell_area, CellRendererState flags)
		{
			if(IsDynamicallyFillList) {
				object obj = MyTreeView.YTreeModel.NodeAtPath(new TreePath(path));
				RefillComboList(obj);
			}
			return base.StartEditing(evnt, widget, path, background_area, cell_area, flags);
		}
	}

	public enum NodeCellRendererColumns
	{
		value,
		title
	}
}

