using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
		NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
				IDisposable toDispose = yTreeModel as IDisposable;
				if(yTreeModel != null) {
					yTreeModel.RenewAdapter -= YTreeModel_RenewAdapter;
				}

				yTreeModel = value;
				if(yTreeModel != null)
					yTreeModel.RenewAdapter += YTreeModel_RenewAdapter;
				Model = yTreeModel == null ? null : yTreeModel.Adapter;
				toDispose?.Dispose();
			}
		}

		public void SetItemsSource<TNode>(IList<TNode> list)
		{
			if(!(ColumnsConfig is FluentColumnsConfig<TNode>))
				throw new InvalidCastException("Type of TNode in IList<TNode> will be type TNode of FluentColumnsConfig<TNode>"
												+ $" {typeof(TNode)} != {ColumnsConfig.GetType().GetGenericArguments().First()}");

			ItemsDataSource = list;
		}

		void YTreeModel_RenewAdapter(object sender, EventArgs e)
		{
			Model = null;
			Model = YTreeModel.Adapter;
		}

		public yTreeView()
		{
			Binding = new BindingControler<yTreeView>(this, new Expression<Func<yTreeView, object>>[] {
				(w => w.SelectedRow),
				(w => w.SelectedRows),
			});
			Selection.Changed += Selection_Changed;
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
					} else if(cell is CellRendererToggle) {
						(cell as CellRendererToggle).Toggled += OnToggledCell;
					}

					var canNextCell = cell as INodeCellRendererCanGoNextCell;
					if(canNextCell != null && (canNextCell.IsEnterToNextCell || col.IsEnterToNextCell)) {
						canNextCell.EditingStarted += CanNextCell_EditingStarted;
					}

					if(cell is ISelfGetNodeRenderer selfGetNodeRenderer)
						selfGetNodeRenderer.GetNodeFunc = path => YTreeModel.NodeAtPath(new TreePath(path));

					tvc.PackStart(cell, render.IsExpand);
					tvc.SetCellDataFunc(cell, NodeRenderColumnFunc);
				}
				AppendColumn(tvc);
			}

			if(ColumnsConfig.ConfiguredColumns.Any(x => x.HasToolTip))
				HasTooltip = true;
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

		#region Edit Functions
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
						try {
							if (cell.EditingValueConverter == null) {
								if(cell.DataPropertyInfo.PropertyType == typeof(decimal)) {
									string val = args.NewText.Replace('.', ',');
									newval = Convert.ChangeType(val, cell.DataPropertyInfo.PropertyType, new CultureInfo("ru-RU"));
								} else {
									newval = Convert.ChangeType(args.NewText, cell.DataPropertyInfo.PropertyType);
								}
							} else
								newval = cell.EditingValueConverter.ConvertBack(args.NewText, cell.DataPropertyInfo.PropertyType, null, null);
						}
						catch (FormatException ex) {
							logger.Warn(ex,"'{0}' is not number", args.NewText);
						}
						catch (OverflowException ex) {
							logger.Warn(ex,"'{0}' is out of range", args.NewText);
						}

						if(newval != null)
						{
							var spinBtn = Children.FirstOrDefault();
							if(spinBtn is SpinButton spin && spin.Adjustment != null)
							{
								CheckAdjustment(ref newval, spin.Adjustment);
							}
							cell.DataPropertyInfo.SetValue(obj, newval, null);
						}
					}
				}
			}
		}

		private static void CheckAdjustment(ref object newval, Adjustment spinAdjustment)
		{
			switch(newval)
			{
				case int intVal:
					var intUpper = (int)spinAdjustment.Upper;
					var intLower = (int)spinAdjustment.Lower;
					if(intVal > intUpper) { newval = intUpper; }
					if(intVal < intLower) { newval = intLower; }
					break;
				case short shortVal:
					var shortUpper = (short)spinAdjustment.Upper;
					var shortLower = (short)spinAdjustment.Lower;
					if(shortVal > shortUpper) { newval = shortUpper; }
					if(shortVal < shortLower) { newval = shortLower; }
					break;
				case long longVal:
					var longUpper = (long)spinAdjustment.Upper;
					var longLower = (long)spinAdjustment.Lower;
					if(longVal > longUpper) { newval = longUpper; }
					if(longVal < longLower) { newval = longLower; }
					break;
				case float floatVal:
					var floatUpper = (float)spinAdjustment.Upper;
					var floatLower = (float)spinAdjustment.Lower;
					if(floatVal > floatUpper) { newval = floatUpper; }
					if(floatVal < floatLower) { newval = floatLower; }
					break;
				case double doubleVal:
					var doubleUpper = spinAdjustment.Upper;
					var doubleLower = spinAdjustment.Lower;
					if(doubleVal > doubleUpper) { newval = doubleUpper; }
					if(doubleVal < doubleLower) { newval = doubleLower; }
					break;
				case decimal decimalVal:
					var decimalUpper = (decimal)spinAdjustment.Upper;
					var decimalLower = (decimal)spinAdjustment.Lower;
					if(decimalVal > decimalUpper) { newval = decimalUpper; }
					if(decimalVal < decimalLower) { newval = decimalLower; }
					break;
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
		#endregion

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

		#region Selection

		#region Свойства можно биндить на viewModel
		public object[] SelectedRows {
			get => GetSelectedObjects();
			set => SelectObject(value);
		}

		public object SelectedRow {
			get => GetSelectedObject();
			set => SelectObject(value);
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			if(Selection.Mode == SelectionMode.Multiple)
				Binding.FireChange(x => x.SelectedRows);
			else
				Binding.FireChange(x => x.SelectedRow);
		}

		#endregion

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
		#endregion

		#region Tooltip

		protected override bool OnQueryTooltip(int x, int y, bool keyboard_tooltip, Tooltip tooltip)
		{
			ConvertWidgetToBinWindowCoords(x, y, out int binX, out int binY);	
			GetPathAtPos(binX, binY, out TreePath path, out TreeViewColumn column);

			var columnConf = ColumnsConfig.ConfiguredColumns.FirstOrDefault(c => c.TreeViewColumn == column);
			if(columnConf != null && columnConf.HasToolTip) {
				object node = YTreeModel.NodeAtPath(path);
				var text = columnConf.GetTooltipText(node);
				tooltip.Text = text;
				return text != null;
			}

			return base.OnQueryTooltip(x, y, keyboard_tooltip, tooltip);
		}

		#endregion

		public override void Destroy() {
			if(ColumnsConfig != null) {
				foreach(var col in ColumnsConfig.ConfiguredColumns) {
					col.ClearProperties();
				}
			}

			base.Destroy();
			if(YTreeModel is IDisposable model) {
				model.Dispose();
			}
		}
	}
}

