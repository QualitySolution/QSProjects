using System;
using System.Collections.Generic;
using System.Reflection;
using Gamma.Binding;
using Gdk;
using Gtk;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererCombo<TNode, TItem> : CellRendererCombo, INodeCellRendererCombo {
		private readonly ListStore _comboListStore;
		public List<Action<NodeCellRendererCombo<TNode, TItem>, TNode>> LambdaSetters = new List<Action<NodeCellRendererCombo<TNode, TItem>, TNode>>();

		public PropertyInfo DataPropertyInfo { get; set; }

		public IValueConverter EditingValueConverter { get; set; }

		public Func<TItem, string> DisplayFunc { get; set; }

		public Func<TItem, string> DisplayListFunc { get; set; }

		public yTreeView MyTreeView { get; set; }

		public Func<TNode, TItem, bool> HideItemFunc { get; set; }

		public bool IsDynamicallyFillList { get; set; }

		public Func<TNode, IList<TItem>> ItemsListFunc;

		public IList<TItem> Items { get; set; }

		public string EmptyValueTitle { get; set; }

		public NodeCellRendererCombo()
		{
			HasEntry = false;
			_comboListStore = new ListStore(typeof(TItem), typeof(string));
			Model = _comboListStore;
		}

		public void RenderNode(object node)
		{
			if(node is TNode typpedNode) {
				var propValue = (TItem)DataPropertyInfo.GetValue(typpedNode, null);
				if(propValue != null)
					Text = DisplayFunc == null ? propValue.ToString() : DisplayFunc(propValue);
				else
					Text = string.Empty;

				LambdaSetters.ForEach(a => a.Invoke(this, typpedNode));
			}
		}

		public override CellEditable StartEditing(Event evnt, Widget widget, string path, Rectangle background_area, Rectangle cell_area, CellRendererState flags)
		{
			if(IsDynamicallyFillList) {
				var obj = MyTreeView.YTreeModel.NodeAtPath(new TreePath(path));
				UpdateComboList((TNode)obj);
			}
			
			return Model.IterNChildren() > 0 ? base.StartEditing(evnt, widget, path, background_area, cell_area, flags) : null;
		}

		public void UpdateComboList(TNode node)
		{
			_comboListStore.Clear();
			
			if(EmptyValueTitle != null)
				_comboListStore.AppendValues(default(TItem), EmptyValueTitle);

			var items = ItemsListFunc?.Invoke(node) ?? Items;

			foreach(var item in items) {

				if(HideItemFunc != null && HideItemFunc(node, item))
					continue;

				if(DisplayListFunc != null)
					_comboListStore.AppendValues(item, DisplayListFunc(item));
				else if(DisplayFunc != null)
					_comboListStore.AppendValues(item, DisplayFunc(item));
				else
					_comboListStore.AppendValues(item, item.ToString());
			}

			TextColumn = (int)NodeCellRendererColumns.title;
		}

		public override void Dispose() {
			_comboListStore.Clear();
			_comboListStore.Dispose();
			
			base.Dispose();
		}
	}

	public enum NodeCellRendererColumns
	{
		value,
		title
	}
}

