using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class ComboRendererMapping<TNode, TItem> : RendererMappingBase<NodeCellRendererCombo<TNode, TItem>, TNode>
	{
		private readonly NodeCellRendererCombo<TNode,TItem> _cellRenderer = new NodeCellRendererCombo<TNode, TItem>();

		public ComboRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, TItem>> dataProperty)
			: base(column)
		{
			var prop = PropertyUtil.GetPropertyInfo (dataProperty);

			if(prop == null)
				throw new InvalidProgramException ();

			_cellRenderer.DataPropertyInfo = prop;
		}

		public ComboRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererCombo<TNode, TItem>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		#region FluentConfig

		public ComboRendererMapping<TNode, TItem> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}
		
		public ComboRendererMapping<TNode, TItem> WrapWidth(int width)
        {
        	_cellRenderer.WrapWidth = width;
        	return this;
        }

		public ComboRendererMapping<TNode, TItem> AddSetter(Action<NodeCellRendererCombo<TNode, TItem>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		/// <summary>
		/// Set render function, always before call FillItems.
		/// </summary>
		public ComboRendererMapping<TNode, TItem> SetDisplayFunc(Func<TItem, string> displayFunc)
		{
			_cellRenderer.DisplayFunc = displayFunc;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> SetDisplayListFunc(Func<TItem, string> displayListFunc)
		{
			_cellRenderer.DisplayListFunc = displayListFunc;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> Editing (bool on = true)
		{
			_cellRenderer.Editable = on;
			return this;
		}
		
		public ComboRendererMapping<TNode, TItem> EditedEvent(EditedHandler handler)
		{
			_cellRenderer.Edited += handler;
			AddHandler(handler);
			return this;
		}

		public ComboRendererMapping<TNode, TItem> HasEntry (bool on = true)
		{
			_cellRenderer.HasEntry = on;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public ComboRendererMapping<TNode, TItem> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		/// <summary>
		/// Hides values from combobox by condition from function.
		/// </summary>
		/// <returns></returns>
		/// <param name="func">Func</param>
		public ComboRendererMapping<TNode, TItem> HideCondition(Func<TNode, TItem, bool> func)
		{
			_cellRenderer.IsDynamicallyFillList = true;
			_cellRenderer.HideItemFunc = func;
			return this;
		}

		/// <summary>
		/// Fill combobox by items.
		/// </summary>
		/// <param name="itemsList">Items list.</param>
		/// <param name="emptyValueTitle">Title for empty value, if set combobox display first item with default value of type(for class is null), and can user set empty value</param>
		public ComboRendererMapping<TNode, TItem> FillItems(IList<TItem> itemsList, string emptyValueTitle = null)
		{
			_cellRenderer.EmptyValueTitle = emptyValueTitle;
			_cellRenderer.Items = itemsList;
			_cellRenderer.UpdateComboList(default(TNode));

			return this;
		}

		public ComboRendererMapping<TNode, TItem> DynamicFillListFunc(Func<TNode, IList<TItem>> func, string emptyValueTitle = null)
		{
			_cellRenderer.EmptyValueTitle = emptyValueTitle;
			_cellRenderer.IsDynamicallyFillList = true;
			_cellRenderer.ItemsListFunc = func;

			return this;
		}

		#endregion
		
		public override void Dispose() {
			if(_cellRenderer != null) {
				_cellRenderer.Dispose();
				
				foreach(var eventInfo in _cellRenderer.GetType().GetEvents()) {
					if(EventHandlers.TryGetValue(eventInfo.EventHandlerType, out var handlers)) {
						foreach(var handler in handlers) {
							eventInfo.RemoveEventHandler(_cellRenderer, (Delegate)handler);
						}
					}
				}
				
				base.Dispose();
			}
		}
	}
}
