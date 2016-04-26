using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class TextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		List<Action<NodeCellRendererText<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererText<TNode>, TNode>>();
		public string DataPropertyName { get; set;}
		private NodeCellRendererText<TNode> cellRenderer = new NodeCellRendererText<TNode> ();

		public TextRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, string>> dataProperty)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo (dataProperty);			
			cellRenderer.LambdaSetters.Add ((c, n) => c.Text = dataProperty.Compile ().Invoke (n));
		}

		public TextRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererText<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}
			
		#endregion

		public TextRendererMapping<TNode> Editable(bool on=true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public TextRendererMapping<TNode> Sensitive(bool on=true)
		{
			cellRenderer.Sensitive = on;
			return this;
		}

		public TextRendererMapping<TNode> AddSetter(Action<NodeCellRendererText<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
	}
}

