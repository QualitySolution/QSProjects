using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class DateRendererMapping<TNode> : RendererMappingBase<NodeCellRendererDate<TNode>, TNode>
	{
		private NodeCellRendererDate<TNode> cellRenderer = new NodeCellRendererDate<TNode> ();

		public DateRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, DateTime?>> getDataExp)
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
			cellRenderer.LambdaSetters.Add ((c, n) => c.Text = DateToText(getter(n)));
		}

		public DateRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
		}

		#region Helpers

		string DateToText(DateTime? date) => date?.ToShortDateString();

		#endregion
		
		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererDate<TNode>, TNode> commonSet)
		{
			cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		#region FluentConfig

		public DateRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public DateRendererMapping<TNode> Editable(bool on=true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		public DateRendererMapping<TNode> Background(string color)
		{
			cellRenderer.Background = color;
			return this;
		}

		public DateRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			cellRenderer.WrapMode = mode;
			return this;
		}

		public DateRendererMapping<TNode> WrapWidth(int width)
		{
			cellRenderer.WrapWidth = width;
			return this;
		}

		public DateRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}

		public DateRendererMapping<TNode> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}
		
		public DateRendererMapping<TNode> YAlign(float alignment)
		{
			cellRenderer.Yalign = alignment;
			return this;
		}

		public DateRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			cellRenderer.SearchHighlight = on;
			return this;
		}

		public DateRendererMapping<TNode> Sensitive(bool on=true)
		{
			cellRenderer.Sensitive = on;
			return this;
		}

		public DateRendererMapping<TNode> AddSetter(Action<NodeCellRendererDate<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public DateRendererMapping<TNode> EditingStartedEvent (Gtk.EditingStartedHandler handler)
		{
			cellRenderer.EditingStarted += handler;
			return this;
		}

		public DateRendererMapping<TNode> EditedEvent (Gtk.EditedHandler handler)
		{
			cellRenderer.Edited += handler;
			return this;
		}

		#endregion
	}
}

