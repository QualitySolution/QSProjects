using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class DateRendererMapping<TNode> : RendererMappingBase<NodeCellRendererDate<TNode>, TNode>
	{
		private readonly NodeCellRendererDate<TNode> _cellRenderer = new NodeCellRendererDate<TNode> ();

		public DateRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, DateTime?>> getDataExp)
			: base(column)
		{
			_cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo(getDataExp);

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
			_cellRenderer.LambdaSetters.Add ((c, n) => c.Text = DateToText(getter(n)));
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
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererDate<TNode>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
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
			_cellRenderer.Editable = on;
			return this;
		}

		public DateRendererMapping<TNode> Background(string color)
		{
			_cellRenderer.Background = color;
			return this;
		}

		public DateRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			_cellRenderer.WrapMode = mode;
			return this;
		}

		public DateRendererMapping<TNode> WrapWidth(int width)
		{
			_cellRenderer.WrapWidth = width;
			return this;
		}

		public DateRendererMapping<TNode> WidthChars(int widthChars)
		{
			_cellRenderer.WidthChars = widthChars;
			return this;
		}

		public DateRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public DateRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		public DateRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			_cellRenderer.SearchHighlight = on;
			return this;
		}

		public DateRendererMapping<TNode> Sensitive(bool on=true)
		{
			_cellRenderer.Sensitive = on;
			return this;
		}

		public DateRendererMapping<TNode> AddSetter(Action<NodeCellRendererDate<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public DateRendererMapping<TNode> EditingStartedEvent (Gtk.EditingStartedHandler handler)
		{
			_cellRenderer.EditingStarted += handler;
			AddHandler(handler);
			return this;
		}

		public DateRendererMapping<TNode> EditedEvent (Gtk.EditedHandler handler)
		{
			_cellRenderer.Edited += handler;
			AddHandler(handler);
			return this;
		}

		#endregion
		
		public override void Dispose() {
			if(_cellRenderer != null) {
				foreach(var eventInfo in _cellRenderer.GetType().GetEvents()) {
					if(EventHandlers.TryGetValue(eventInfo.EventHandlerType, out var handlers)) {
						foreach(var handler in handlers) {
							eventInfo.RemoveEventHandler(_cellRenderer, (Delegate)handler);
						}
					}
				}
				
				base.Dispose();
			}
		}
	}
}

