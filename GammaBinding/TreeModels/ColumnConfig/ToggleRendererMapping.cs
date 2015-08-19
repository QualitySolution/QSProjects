using System;
using System.Linq.Expressions;
using GammaBinding.Gtk.Cells;

namespace GammaBinding.ColumnConfig
{
	public class ToggleRendererMapping<TNode> : RendererMappingBase<NodeCellRendererToggle<TNode>, TNode>
	{
		private NodeCellRendererToggle<TNode> cellRenderer = new NodeCellRendererToggle<TNode>();

		public ToggleRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, bool>> dataProperty)
			: base(column)
		{
			//cellRenderer.DataPropertyName = PropertyUtil.GetName<TNode> (dataProperty);
			cellRenderer.LambdaSetters.Add ((c, n) => c.Active = dataProperty.Compile ().Invoke (n));
		}

		public ToggleRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererToggle<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}

		#endregion

		public ToggleRendererMapping<TNode> AddSetter(Action<NodeCellRendererToggle<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
			
		public ToggleRendererMapping<TNode> Editing (bool on = true)
		{
			cellRenderer.Activatable = on;
			return this;
		}
	}
}

