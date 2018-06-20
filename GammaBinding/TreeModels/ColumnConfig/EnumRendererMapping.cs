using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class EnumRendererMapping<TNode, TItem> : RendererMappingBase<NodeCellRendererCombo<TNode, TItem>, TNode> where TItem : struct, IConvertible
	{
		private NodeCellRendererCombo<TNode, TItem> cellRenderer = new NodeCellRendererCombo<TNode, TItem>();

		public EnumRendererMapping(ColumnMapping<TNode> column, Expression<Func<TNode, TItem>> dataProperty, Enum[] excludeItems)
			: base(column)
		{
			var prop = PropertyUtil.GetPropertyInfo(dataProperty);

			if(prop == null || !prop.PropertyType.IsEnum)
				throw new InvalidProgramException();

			cellRenderer.DataPropertyInfo = prop;

			FillRendererByEnum(default(TNode), prop.PropertyType, excludeItems);

			cellRenderer.FillComboListFunc = node => FillRendererByEnum(node, prop.PropertyType, excludeItems);

			cellRenderer.DisplayFunc = e => (e as Enum).GetEnumTitle();
		}

		public EnumRendererMapping(ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent(Action<NodeCellRendererCombo<TNode, TItem>, TNode> commonSet)
		{
			AddSetter(commonSet);
		}

		#endregion

		public EnumRendererMapping<TNode, TItem> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public EnumRendererMapping<TNode, TItem> AddSetter(Action<NodeCellRendererCombo<TNode, TItem>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add(setter);
			return this;
		}

		public EnumRendererMapping<TNode, TItem> Editing(bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public EnumRendererMapping<TNode, TItem> HasEntry(bool on = true)
		{
			cellRenderer.HasEntry = on;
			return this;
		}

		public EnumRendererMapping<TNode, TItem> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}

		/// <summary>
		/// Hides values from combobox by condition from function.
		/// </summary>
		/// <returns></returns>
		/// <param name="func">Func</param>
		public EnumRendererMapping<TNode, TItem> HideCondition(Func<TNode, TItem, bool> func)
		{
			cellRenderer.HideItemFunc = func;
			return this;
		}

		private void FillRendererByEnum(TNode node, Type enumType, Enum[] excludeItems)
		{
			ListStore comboListStore = new ListStore(enumType, typeof(string));

			foreach(FieldInfo info in enumType.GetFields()) {
				if(info.Name.Equals("value__"))
					continue;
				if(excludeItems != null && excludeItems.Length > 0 && excludeItems.Contains(info.GetValue(null)))
					continue;
				if(cellRenderer.HideItemFunc != null && cellRenderer.HideItemFunc(node, (TItem)info.GetValue(null)))
					continue;

				string title = info.GetEnumTitle();
				comboListStore.AppendValues(info.GetValue(null), title);
			}

			cellRenderer.Model = comboListStore;
			cellRenderer.TextColumn = (int)NodeCellRendererColumns.title;
		}
	}
}