using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;

namespace Gamma.ColumnConfig
{
	public class ProgressRendererMapping<TNode> : RendererMappingBase<NodeCellRendererProgress<TNode>, TNode>
	{
		List<Action<NodeCellRendererProgress<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererProgress<TNode>, TNode>>();
		public string DataPropertyName { get; set;}

		public ProgressRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, int>> getdataExp)
			: base(column)
		{
			//DataPropertyName = PropertyUtil.GetName<TNode> (dataProperty);
			var getter = getdataExp.Compile();
			LambdaSetters.Add ((c, n) => c.Value = getter (n));
		}

		public ProgressRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			var cell = new NodeCellRendererProgress<TNode> ();
			//cell.DataPropertyName = DataPropertyName;
			cell.LambdaSetters = LambdaSetters;
			return cell;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererProgress<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}

		#endregion

		public ProgressRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public ProgressRendererMapping<TNode> AddSetter(Action<NodeCellRendererProgress<TNode>, TNode> setter)
		{
			LambdaSetters.Add (setter);
			return this;
		}
	}
}

