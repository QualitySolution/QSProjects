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
			AddSetter (commonSet);
		}

		#endregion

		public ComboRendererMapping<TNode, TItem> Tag(object tag)
		{
			this.tag = tag;
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

		public ComboRendererMapping<TNode, TItem> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> HasEntry (bool on = true)
		{
			cellRenderer.HasEntry = on;
			return this;
		}

		public ComboRendererMapping<TNode, TItem> FillItems(IList<TItem> itemsList)
		{
			FillRendererByList (itemsList);
			return this;
		}

		private void FillRendererByList(IList<TItem> itemsList)
		{
			ListStore comboListStore = new ListStore (typeof(TItem), typeof(string));

			foreach (var item in itemsList) {
				if(cellRenderer.DisplayFunc == null)
					comboListStore.AppendValues (item, item.ToString ());
				else
					comboListStore.AppendValues (item, cellRenderer.DisplayFunc(item));
			}

			cellRenderer.TextColumn = (int)NodeCellRendererColumns.title;
			cellRenderer.Model = comboListStore;
		}
	}
}