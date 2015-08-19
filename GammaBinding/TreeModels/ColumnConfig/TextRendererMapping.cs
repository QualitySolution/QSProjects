using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GammaBinding.Gtk.Cells;

namespace GammaBinding.ColumnConfig
{
	public class TextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		List<Action<NodeCellRendererText<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererText<TNode>, TNode>>();
		public string DataPropertyName { get; set;}

		public TextRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, string>> dataProperty)
			: base(column)
		{
			//DataPropertyName = PropertyUtil.GetName<TNode> (dataProperty);
			LambdaSetters.Add ((c, n) => c.Text = dataProperty.Compile ().Invoke (n));
		}

		public TextRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			var cell = new NodeCellRendererText<TNode> ();
			cell.DataPropertyName = DataPropertyName;
			cell.LambdaSetters = LambdaSetters;
			return cell;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererText<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}
			
		#endregion

		public TextRendererMapping<TNode> AddSetter(Action<NodeCellRendererText<TNode>, TNode> setter)
		{
			LambdaSetters.Add (setter);
			return this;
		}
	}
}

