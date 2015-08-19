using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class ComboRendererMapping<TNode> : RendererMappingBase<NodeCellRendererCombo<TNode>, TNode>
	{
		private NodeCellRendererCombo<TNode> cellRenderer = new NodeCellRendererCombo<TNode>();

		public ComboRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> dataProperty)
			: base(column)
		{
			cellRenderer.DataPropertyName = PropertyUtil.GetName<TNode> (dataProperty);

			MemberExpression memberExpr = dataProperty.Body as MemberExpression;
			if (memberExpr == null) {
				UnaryExpression unaryExpr = dataProperty.Body as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}
			if (memberExpr == null)
				throw new InvalidProgramException ();

			var prop = memberExpr.Member as PropertyInfo;

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

		protected override void SetSetterSilent (Action<NodeCellRendererCombo<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}

		#endregion

		public ComboRendererMapping<TNode> AddSetter(Action<NodeCellRendererCombo<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public ComboRendererMapping<TNode> SetDisplayFunc(Func<object, string> displayFunc)
		{
			cellRenderer.DisplayFunc = displayFunc;
			return this;
		}
			
		public ComboRendererMapping<TNode> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public ComboRendererMapping<TNode> HasEntry (bool on = true)
		{
			cellRenderer.HasEntry = on;
			return this;
		}

		public ComboRendererMapping<TNode> FillItems<TProperty>(IList<TProperty> itemsList)
		{
			FillRendererByList (itemsList);
			return this;
		}

		private void FillRendererByList<TProperty>(IList<TProperty> itemsList)
		{
			ListStore comboListStore = new ListStore (typeof(TProperty), typeof(string));

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