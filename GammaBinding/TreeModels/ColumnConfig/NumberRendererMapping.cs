using System;
using System.Linq.Expressions;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class NumberRendererMapping<TNode> : RendererMappingBase<NodeCellRendererSpin<TNode>, TNode>
	{
		private NodeCellRendererSpin<TNode> cellRenderer = new NodeCellRendererSpin<TNode>();

		public NumberRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> dataProperty)
			: base(column)
		{
			cellRenderer.DataPropertyName = PropertyUtil.GetName<TNode> (dataProperty);
			cellRenderer.LambdaSetters.Add ((c, n) => c.Text = String.Format ("{0:" + String.Format ("F{0}", c.Digits) + "}", dataProperty.Compile ().Invoke (n)));
		}

		public NumberRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererSpin<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}

		#endregion

		public NumberRendererMapping<TNode> AddSetter(Action<NodeCellRendererSpin<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public NumberRendererMapping<TNode> Digits(uint digits)
		{
			cellRenderer.Digits = digits;
			return this;
		}

		public NumberRendererMapping<TNode> Adjustment(Adjustment adjustment)
		{
			cellRenderer.Adjustment = adjustment;
			return this;
		}

		public NumberRendererMapping<TNode> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public NumberRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}
	}
}

