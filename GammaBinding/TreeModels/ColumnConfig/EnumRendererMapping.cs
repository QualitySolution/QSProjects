using System;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class EnumRendererMapping<TNode> : RendererMappingBase<NodeCellRendererCombo<TNode>, TNode>
	{
		private NodeCellRendererCombo<TNode> cellRenderer = new NodeCellRendererCombo<TNode>();

		public EnumRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> dataProperty)
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

			if(prop == null || !prop.PropertyType.IsEnum)
				throw new InvalidProgramException ();

			cellRenderer.DataPropertyInfo = prop;

			FillRendererByEnum (prop.PropertyType);

			cellRenderer.DisplayFunc = e => ((Enum)e).GetEnumTitle ();
		}

		public EnumRendererMapping (ColumnMapping<TNode> column)
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

		public EnumRendererMapping<TNode> AddSetter(Action<NodeCellRendererCombo<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
			
		public EnumRendererMapping<TNode> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public EnumRendererMapping<TNode> HasEntry (bool on = true)
		{
			cellRenderer.HasEntry = on;
			return this;
		}

		private void FillRendererByEnum(Type enumType)
		{
			ListStore comboListStore = new ListStore (enumType, typeof(string));

			foreach (FieldInfo info in enumType.GetFields()) {
				if (info.Name.Equals("value__"))
					continue;
				string title = info.GetEnumTitle ();
				comboListStore.AppendValues (info.GetValue (null), title);
			}
				
			cellRenderer.Model = comboListStore;
			cellRenderer.TextColumn = (int)NodeCellRendererColumns.title;
		}
	}
}