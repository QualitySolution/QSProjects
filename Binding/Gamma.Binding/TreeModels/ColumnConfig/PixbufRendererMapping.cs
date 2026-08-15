using System;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gdk;

namespace Gamma.ColumnConfig
{
	public class PixbufRendererMapping<TNode> : RendererMappingBase<NodeCellRendererPixbuf<TNode>, TNode>
	{
		private readonly NodeCellRendererPixbuf<TNode> _cellRenderer = new NodeCellRendererPixbuf<TNode> ();

		public PixbufRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, Pixbuf>> getDataExp)
			: base(column)
		{
			_cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo (getDataExp);
			var getter = getDataExp.Compile();
			_cellRenderer.LambdaSetters.Add ((c, n) => c.Pixbuf = getter (n));
		}

		public PixbufRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererPixbuf<TNode>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		public PixbufRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public PixbufRendererMapping<TNode> Sensitive(bool on=true)
		{
			_cellRenderer.Sensitive = on;
			return this;
		}

		public PixbufRendererMapping<TNode> AddSetter(Action<NodeCellRendererPixbuf<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
	}
}

