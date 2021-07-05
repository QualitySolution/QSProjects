using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings;
using System.Globalization;
using System.Linq;
using Gamma.Binding;
using Gamma.Binding.Core;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.GtkWidgets
{
	[ToolboxItem(true)]
	[Category("Gamma Gtk")]
	public class yTreeView : TreeView
	{
		public BindingControler<yTreeView> Binding { get; private set; }

		IColumnsConfig columnsConfig;
		public IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
			set {
				if(columnsConfig == value)
					return;
				columnsConfig = value;
				VerifyNodeTypes();
				ReconfigureColumns();
			}
		}

		public FluentColumnsConfig<TNode> CreateFluentColumnsConfig<TNode>()
		{
			return new FluentColumnsConfig<TNode>(this);
		}

		private string[] searchHighlightTexts;

		public string[] SearchHighlightTexts {
			get => searchHighlightTexts; 
			set {
				if(searchHighlightTexts != null && searchHighlightTexts.Any(x => String.IsNullOrEmpty(x)))
					throw new ArgumentException("Строки поиска не должны содержать пустые значения. Это затрудняет разбор и замедляет подсветку значений.", nameof(SearchHighlightTexts));
				searchHighlightTexts = value;
			}
		}

		public string SearchHighlightText {
			get {
				return SearchHighlightTexts != null && SearchHighlightTexts.Length > 0
					? SearchHighlightTexts[0] : String.Empty;
			}
			set {
				if(String.IsNullOrEmpty(value))
					SearchHighlightTexts = null;
				else
					SearchHighlightTexts = new string[] { value };
			}
		}

		object itemsDataSource;

		public virtual object ItemsDataSource {
			get { return itemsDataSource; }
			set {
				if(value == null) {
					Model = null;
					return;
				}
				var list = (value as IList);
				if(list == null)
					throw new NotSupportedException(String.Format(
						"Type '{0}' is not supported. Data source must implement IList.",
						value.GetType()
					));

				if(value is IObservableList) {
					if(Reorderable)
						YTreeModel = new ObservableListReorderableTreeModel(value as IObservableList);
					else
						YTreeModel = new ObservableListTreeModel(value as IObservableList);
				}
				else { 
					YTreeModel = new ListTreeModel(list);
				}
				itemsDataSource = value;
				VerifyNodeTypes();
			}
		}

		private IyTreeModel yTreeModel;

		public IyTreeModel YTreeModel {
			get {
				return yTreeModel;
			}
			set {
				if(yTreeModel == value)
					return;
				if(yTreeModel != null)
					yTreeModel.RenewAdapter -= YTreeModel_RenewAdapter;
				yTreeModel = value;
				if(yTreeModel != null)
					yTreeModel.RenewAdapter += YTreeModel_RenewAdapter;
				Model = yTreeModel == null ? null : yTreeModel.Adapter;
			}
		}

		public void SetItemsSource<TNode>(IList<TNode> list)
		{
			if(!(ColumnsConfig is FluentColumnsConfig<TNode>))
				throw new InvalidCastException("Type of TNode in IList<TNode> will be type TNode of FluentColumnsConfig<TNode>"
												+ $" {typeof(TNode)} != {ColumnsConfig.GetType().GetGenericArguments().First()}");

			ItemsDataSource = list;
		}

		void VerifyNodeTypes()
		{
			if(itemsDataSource == null || columnsConfig == null)
				return;
			if(!itemsDataSource.GetType().IsGenericType)
				return;

			var dataSourceType = itemsDataSource.GetType().GetGenericArguments()[0];
			if(dataSourceType is System.Object)
				return;
			var columnsConfigType = columnsConfig.GetType().GetGenericArguments()[0];
			if(dataSourceType != columnsConfigType && !dataSourceType.IsSubclassOf(columnsConfigType)) {
				throw new ArgumentException(String.Format(
						"Data source element type '{0}' does not match columns configuration type '{1}'.",
						dataSourceType,
						columnsConfigType
					));
			}
		}

		void YTreeModel_RenewAdapter(object sender, EventArgs e)
		{
			Model = null;
			Model = YTreeModel.Adapter;
		}

		public yTreeView()
		{
			Binding = new BindingControler<yTreeView>(this);
		}

		void ReconfigureColumns()
		{
			while(Columns.Length > 0)
				RemoveColumn(Columns[0]);

			foreach(var col in ColumnsConfig.ConfiguredColumns) {
				TreeViewColumn tvc = col.TreeViewColumn;

				foreach(var render in col.ConfiguredRenderers) {
					var cell = render.GetRenderer() as CellRenderer;
					if(cell is CellRendererSpin) {
						(cell as CellRendererSpin).EditingStarted += OnNumbericNodeCellEditingStarted;
						(cell as CellRendererSpin).Edited += NumericNodeCellEdited;
					} else if(cell is CellRendererCombo) {
						(cell as CellRendererCombo).Edited += ComboNodeCellEdited;
						(cell as INodeCellRendererCombo).MyTreeView = this;
					} else if(cell is CellRendererText) {
						(cell as CellRendererText).Edited += TextNodeCellEdited;
					}
					if(cell is CellRendererToggle) {
						(cell as CellRendererToggle).Toggled += OnToggledCell;
					}

					var canNextCell = cell as INodeCellRendererCanGoNextCell;
					if(canNextCell != null && (canNextCell.IsEnterToNextCell || col.IsEnterToNextCell)) {
						canNextCell.EditingStarted += CanNextCell_EditingStarted;
					}

					tvc.PackStart(cell, render.IsExpand);
					tvc.SetCellDataFunc(cell, NodeRenderColumnFunc);
				}
				AppendColumn(tvc);
			}
		}

		private CellRenderer editingCell;

		void CanNextCell_EditingStarted(object o, EditingStartedArgs args)
		{
			editingCell = o as CellRenderer;
			var editingWidget = args.Editable as Widget;
			editingWidget.AddEvents((int)Gdk.EventMask.KeyPressMask);
			editingWidget.KeyPressEvent += EditableToNextCell_KeyPressEvent;
		}

		[GLib.ConnectBefore]
		void EditableToNextCell_KeyPressEvent(object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Return) {
				TreeIter iter;
				Selection.GetSelected(out iter);
				if(Model.IterNext(ref iter)) {
					var path = Model.GetPath(iter);
					var column = FindColumn(editingCell);
					Application.Invoke(delegate {
						SetCursorOnCell(path, column, editingCell, true);
					});
				}
			}
		}

		TreeViewColumn FindColumn(CellRenderer cell)
		{
			return Columns.FirstOrDefault(c => c.CellRenderers.Contains(cell));
		}

		void OnToggledCell(object o, ToggledArgs args)
		{
			var cell = o as INodeCellRenderer;
			if(cell != null) {
				object obj = YTreeModel.NodeAtPath(new TreePath(args.Path));
				if(cell.DataPropertyInfo != null) {
					object propValue = cell.DataPropertyInfo.GetValue(obj, null);

					cell.DataPropertyInfo.SetValue(obj, !Convert.ToBoolean(propValue), null);
				}
			}
		}

		private void OnNumbericNodeCellEditingStarted(object o, Gtk.EditingStartedArgs args)
		{
			var cell = o as INodeCellRenderer;
			if(cell != null) {
				object obj = YTreeModel.NodeAtPath(new TreePath(args.Path));
				if(cell.DataPropertyInfo != null) {
					object propValue = cell.DataPropertyInfo.GetValue(obj, null);
					var spin = o as CellRendererSpin;
					if(spin != null) {
						//WORKAROUND to fix GTK bug that CellRendererSpin start editing only with integer number
						if(cell.EditingValueConverter == null)
							spin.Adjustment.Value = Convert.ToDouble(propValue);
						else
							spin.Adjustment.Value = (double)cell.EditingValueConverter.Convert(propValue, typeof(double), null, null);
					}
				}
			}
		}

		private void NumericNodeCellEdited(object o, Gtk.EditedArgs args)
		{
			TreeIter iter;

			INodeCellRenderer cell = o as INodeCellRenderer;
			//CellRendererSpin cellSpin =  o as CellRendererSpin;

			if(cell != null) {
				// Resolve path as it was passed in the arguments
				Gtk.TreePath tp = new Gtk.TreePath(args.Path);
				// Change value in the original object
				if(YTreeModel.Adapter.GetIter(out iter, tp)) {
					object obj = YTreeModel.NodeFromIter(iter);

					if(cell.DataPropertyInfo.CanWrite && !String.IsNullOrWhiteSpace(args.NewText)) {
						object newval = null;
						if(cell.EditingValueConverter == null) {
							try {
								if(cell.DataPropertyInfo.PropertyType == typeof(decimal)) {
									string val = args.NewText.Replace('.', ',');
									newval = Convert.ChangeType(val, cell.DataPropertyInfo.PropertyType, new CultureInfo("ru-RU"));
								} else {
									newval = Convert.ChangeType(args.NewText, cell.DataPropertyInfo.PropertyType);
								}
							} catch(FormatException ex) {
								Console.WriteLine("'{0}' is not number", args.NewText);
							}
						} else
							newval = cell.EditingValueConverter.ConvertBack(args.NewText, cell.DataPropertyInfo.PropertyType, null, null);

						if(newval != null)
							cell.DataPropertyInfo.SetValue(obj, newval, null);
					}
				}
			}
		}

		private void ComboNodeCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;

			INodeCellRenderer cell = o as INodeCellRenderer;
			CellRendererCombo combo = o as CellRendererCombo;

			if(cell != null) {
				// Resolve path as it was passed in the arguments
				Gtk.TreePath tp = new Gtk.TreePath(args.Path);
				// Change value in the original object
				if(YTreeModel.Adapter.GetIter(out iter, tp)) {
					object obj = YTreeModel.NodeFromIter(iter);
					if(cell.DataPropertyInfo.CanWrite && !String.IsNullOrWhiteSpace(args.NewText)) {
						foreach(object[] row in (ListStore)combo.Model) {
							if((string)row[(int)NodeCellRendererColumns.title] == args.NewText) {
								cell.DataPropertyInfo.SetValue(obj, row[(int)NodeCellRendererColumns.value], null);
								break;
							}
						}
					}
				}
			}
		}

		private void TextNodeCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;

			INodeCellRenderer cell = o as INodeCellRenderer;
			CellRendererText cellText = o as CellRendererText;

			if(cell != null) {
				// Resolve path as it was passed in the arguments
				Gtk.TreePath tp = new Gtk.TreePath(args.Path);
				// Change value in the original object
				if(YTreeModel.Adapter.GetIter(out iter, tp)) {
					object obj = YTreeModel.NodeFromIter(iter);
					if(cell.DataPropertyInfo != null && cell.DataPropertyInfo.CanWrite) {
						cell.DataPropertyInfo.SetValue(obj, args.NewText, null);
					}
				}
			}
		}

		private void NodeRenderColumnFunc(Gtk.TreeViewColumn aColumn, Gtk.CellRenderer aCell,
			Gtk.TreeModel aModel, Gtk.TreeIter aIter)
		{
			if(aIter.Equals(TreeIter.Zero as object))
				return;

			if(YTreeModel == null) {
				throw new InvalidOperationException($"{nameof(YTreeModel)} can not be null");
			}

			object node = YTreeModel.NodeFromIter(aIter);
			var nodeCell = aCell as INodeCellRenderer;
			if(nodeCell != null) {
				try {
					if(nodeCell is INodeCellRendererHighlighter)
						(nodeCell as INodeCellRendererHighlighter).RenderNode(node, SearchHighlightTexts);
					else
						nodeCell.RenderNode(node);
				} catch(Exception ex) {
					throw new InvalidProgramException($"Exception inside rendering column {aColumn?.Title}.", ex);
				}
			}
		}

		public object[] GetSelectedObjects()
		{
			return GetSelectedObjects<object>();
		}

		public TNode[] GetSelectedObjects<TNode>()
		{
			TreePath[] tp = Selection.GetSelectedRows();
			TNode[] rows = new TNode[tp.Length];
			for(int i = 0; i < rows.Length; i++) {
				rows[i] = (TNode)YTreeModel.NodeAtPath(tp[i]);
			}
			return rows;
		}

		public object GetSelectedObject()
		{
			TreeIter iter;
			if(Selection.GetSelected(out iter))
				return YTreeModel.NodeFromIter(iter);
			else
				return null;
		}

		public TNode GetSelectedObject<TNode>()
		{
			TreeIter iter;
			if(Selection.GetSelected(out iter))
				return (TNode)YTreeModel.NodeFromIter(iter);
			else
				return default(TNode);
		}

		public void SelectObject(object item)
		{
			TreeIter iter = YTreeModel.IterFromNode(item);
			Selection.SelectIter(iter);
		}

		public void SelectObject(object[] items)
		{
			foreach(var item in items) {
				TreeIter iter = YTreeModel.IterFromNode(item);
				Selection.SelectIter(iter);
			}
		}

	}
}

