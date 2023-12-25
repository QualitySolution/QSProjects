using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class TimeRendererMapping<TNode> : RendererMappingBase<NodeCellRendererTime<TNode>, TNode>
	{
		private NodeCellRendererTime<TNode> cellRenderer = new NodeCellRendererTime<TNode> ();

		public TimeRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, TimeSpan?>> getDataExp)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo(getDataExp);

			var properties = FetchPropertyInfoFromExpression.Fetch(getDataExp);

			foreach(var prop in properties)
			{
				var att = prop.GetCustomAttributes(typeof(SearchHighlightAttribute), false);
				if (att.Length > 0)
				{
					SearchHighlight();
					break;
				}
			}

			var getter = getDataExp.Compile();
			cellRenderer.LambdaSetters.Add ((c, n) => c.Text = TimeToText(getter(n)));
		}

		public TimeRendererMapping (ColumnMapping<TNode> column)
			: base(column){}

		#region Helpers

		string TimeToText(TimeSpan? time) => time?.ToString("hh\\:mm");

		#endregion

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererTime<TNode>, TNode> commonSet)
		{
			cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		#region FluentConfig

		public TimeRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public TimeRendererMapping<TNode> Editable(bool on=true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public TimeRendererMapping<TNode> Background(string color)
		{
			cellRenderer.Background = color;
			return this;
		}

		public TimeRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			cellRenderer.WrapMode = mode;
			return this;
		}

		public TimeRendererMapping<TNode> WrapWidth(int width)
		{
			cellRenderer.WrapWidth = width;
			return this;
		}

		public TimeRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}

		public TimeRendererMapping<TNode> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}
		
		public TimeRendererMapping<TNode> YAlign(float alignment)
		{
			cellRenderer.Yalign = alignment;
			return this;
		}

		public TimeRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			cellRenderer.SearchHighlight = on;
			return this;
		}

		public TimeRendererMapping<TNode> Sensitive(bool on=true)
		{
			cellRenderer.Sensitive = on;
			return this;
		}

		public TimeRendererMapping<TNode> AddSetter(Action<NodeCellRendererTime<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public TimeRendererMapping<TNode> EditingStartedEvent (Gtk.EditingStartedHandler handler)
		{
			cellRenderer.EditingStarted += handler;
			return this;
		}

		public TimeRendererMapping<TNode> EditedEvent (Gtk.EditedHandler handler)
		{
			cellRenderer.Edited += handler;
			return this;
		}

		#endregion
	}
}

