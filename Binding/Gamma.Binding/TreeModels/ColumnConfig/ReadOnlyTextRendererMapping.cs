using System;
using Gamma.GtkWidgets.Cells;

namespace Gamma.ColumnConfig
{
	public class ReadOnlyTextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		private NodeCellRendererText<TNode> cellRenderer = new NodeCellRendererText<TNode> ();

		public ReadOnlyTextRendererMapping (ColumnMapping<TNode> column, Func<TNode, string> getTextFunc, bool useMarkup = false)
			: base(column)
		{
			if(useMarkup)
				cellRenderer.LambdaSetters.Add ((c, n) => c.Markup = getTextFunc(n));
			else
				cellRenderer.LambdaSetters.Add ((c, n) => c.Text = getTextFunc(n));
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererText<TNode>, TNode> commonSet)
		{
			cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		#region FluentConfig

		public ReadOnlyTextRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> Background(string color)
		{
			cellRenderer.Background = color;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			cellRenderer.WrapMode = mode;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WrapWidth(int width)
		{
			cellRenderer.WrapWidth = width;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			cellRenderer.SearchHighlight = on;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> Sensitive(bool on=true)
		{
			cellRenderer.Sensitive = on;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> AddSetter(Action<NodeCellRendererText<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
		#endregion
	}
}

