using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class TextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		public string DataPropertyName { get; set;}
		private NodeCellRendererText<TNode> cellRenderer = new NodeCellRendererText<TNode> ();

		public TextRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, string>> dataProperty, bool useMarkup = false)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo(dataProperty);

			var properties = FetchPropertyInfoFromExpression.Fetch(dataProperty);

			foreach(var prop in properties)
			{
				var att = prop.GetCustomAttributes(typeof(SearchHighlightAttribute), false);
				if (att.Length > 0)
				{
					SearchHighlight();
					break;
				}
			}
			if(useMarkup)
				cellRenderer.LambdaSetters.Add ((c, n) => c.Markup = dataProperty.Compile ().Invoke (n));
			else
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

		#region FluentConfig

		public TextRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public TextRendererMapping<TNode> Editable(bool on=true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public TextRendererMapping<TNode> Background(string color)
		{
			cellRenderer.Background = color;
			return this;
		}

		public TextRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			cellRenderer.WrapMode = mode;
			return this;
		}

		public TextRendererMapping<TNode> WrapWidth(int width)
		{
			cellRenderer.WrapWidth = width;
			return this;
		}

		public TextRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}

		public TextRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			cellRenderer.SearchHighlight = on;
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

		public TextRendererMapping<TNode> EditingStartedEvent (Gtk.EditingStartedHandler handler)
		{
			cellRenderer.EditingStarted += handler;
			return this;
		}

		public TextRendererMapping<TNode> EditedEvent (Gtk.EditedHandler handler)
		{
			cellRenderer.Edited += handler;
			return this;
		}

		#endregion
	}
}

