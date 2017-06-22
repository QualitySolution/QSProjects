using System;
using System.Linq.Expressions;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gdk;

namespace Gamma.ColumnConfig
{
	public class PixbufRendererMapping<TNode> : RendererMappingBase<NodeCellRendererPixbuf<TNode>, TNode>
	{
		public string DataPropertyName { get; set;}
		private NodeCellRendererPixbuf<TNode> cellRenderer = new NodeCellRendererPixbuf<TNode> ();

		public PixbufRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, Pixbuf>> dataProperty)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo (dataProperty);			
			cellRenderer.LambdaSetters.Add ((c, n) => c.Pixbuf = dataProperty.Compile ().Invoke (n));
		}

		public PixbufRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererPixbuf<TNode>, TNode> commonSet)
		{
			AddSetter (commonSet);
		}

		#endregion

		public PixbufRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public PixbufRendererMapping<TNode> Sensitive(bool on=true)
		{
			cellRenderer.Sensitive = on;
			return this;
		}

		public PixbufRendererMapping<TNode> AddSetter(Action<NodeCellRendererPixbuf<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
	}
}

