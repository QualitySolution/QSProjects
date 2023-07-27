﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class ComboRendererMapping<TNode, TItem> : RendererMappingBase<NodeCellRendererCombo<TNode, TItem>, TNode>
	{
		private NodeCellRendererCombo<TNode,TItem> cellRenderer = new NodeCellRendererCombo<TNode, TItem>();

		public ComboRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, TItem>> dataProperty)
			: base(column)
		{
			var prop = PropertyUtil.GetPropertyInfo (dataProperty);

			if(prop == null)
				throw new InvalidProgramException ();

			cellRenderer.DataPropertyInfo = prop;
		}

		public ComboRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererCombo<TNode, TItem>, TNode> commonSet)
		{
			cellRenderer.LambdaSetters.Insert(0, commonSet);
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
        	cellRenderer.WrapWidth = width;
        	return this;
        }

		public ComboRendererMapping<TNode, TItem> AddSetter(Action<NodeCellRendererCombo<TNode, TItem>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		/// <summary>
		/// Set render function, always before call FillItems.
		/// </summary>
		public ComboRendererMapping<TNode, TItem> SetDisplayFunc(Func<TItem, string> displayFunc)
		{
			cellRenderer.DisplayFunc = displayFunc;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> SetDisplayListFunc(Func<TItem, string> displayListFunc)
		{
			cellRenderer.DisplayListFunc = displayListFunc;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}
		
		public ComboRendererMapping<TNode, TItem> EditedEvent(EditedHandler handler)
		{
			cellRenderer.Edited += handler;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> HasEntry (bool on = true)
		{
			cellRenderer.HasEntry = on;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}
		
		public ComboRendererMapping<TNode, TItem> YAlign(float alignment)
		{
			cellRenderer.Yalign = alignment;
			return this;
		}

		/// <summary>
		/// Hides values from combobox by condition from function.
		/// </summary>
		/// <returns></returns>
		/// <param name="func">Func</param>
		public ComboRendererMapping<TNode, TItem> HideCondition(Func<TNode, TItem, bool> func)
		{
			cellRenderer.IsDynamicallyFillList = true;
			cellRenderer.HideItemFunc = func;
			return this;
		}

		/// <summary>
		/// Fill combobox by items.
		/// </summary>
		/// <param name="itemsList">Items list.</param>
		/// <param name="emptyValueTitle">Title for empty value, if set combobox dispaly first item with default value of type(for class is null), and can user set empty value</param>
		public ComboRendererMapping<TNode, TItem> FillItems(IList<TItem> itemsList, string emptyValueTitle = null)
		{
			cellRenderer.EmptyValueTitle = emptyValueTitle;
			cellRenderer.Items = itemsList;
			cellRenderer.UpdateComboList(default(TNode));

			return this;
		}

		public ComboRendererMapping<TNode, TItem> DynamicFillListFunc(Func<TNode, IList<TItem>> func)
		{
			cellRenderer.IsDynamicallyFillList = true;
			cellRenderer.ItemsListFunc = func;

			return this;
		}

		#endregion
	}
}
