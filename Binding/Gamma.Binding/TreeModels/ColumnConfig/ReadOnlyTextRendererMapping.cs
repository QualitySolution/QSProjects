using System;
using Gamma.GtkWidgets.Cells;

namespace Gamma.ColumnConfig
{
	public class ReadOnlyTextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		private readonly NodeCellRendererText<TNode> _cellRenderer = new NodeCellRendererText<TNode> ();

		public ReadOnlyTextRendererMapping (ColumnMapping<TNode> column, Func<TNode, string> getTextFunc, bool useMarkup = false)
			: base(column)
		{
			if(useMarkup)
				_cellRenderer.LambdaSetters.Add ((c, n) => c.Markup = getTextFunc(n));
			else
				_cellRenderer.LambdaSetters.Add ((c, n) => c.Text = getTextFunc(n));
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererText<TNode>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
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
			_cellRenderer.Background = color;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			_cellRenderer.WrapMode = mode;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WrapWidth(int width)
		{
			_cellRenderer.WrapWidth = width;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> WidthChars(int widthChars)
		{
			_cellRenderer.WidthChars = widthChars;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public ReadOnlyTextRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			_cellRenderer.SearchHighlight = on;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> Sensitive(bool on=true)
		{
			_cellRenderer.Sensitive = on;
			return this;
		}

		public ReadOnlyTextRendererMapping<TNode> AddSetter(Action<NodeCellRendererText<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
		#endregion
	}
}

