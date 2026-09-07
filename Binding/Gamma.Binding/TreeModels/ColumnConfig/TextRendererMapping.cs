using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class TextRendererMapping<TNode> : RendererMappingBase<NodeCellRendererText<TNode>, TNode>
	{
		private readonly NodeCellRendererText<TNode> _cellRenderer = new NodeCellRendererText<TNode>();
		private Func<TNode, string> _getValueFunc;

		public TextRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, string>> getDataExp, bool useMarkup = false)
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

			_getValueFunc = getDataExp.Compile();
			if(useMarkup)
				_cellRenderer.LambdaSetters.Add ((c, n) => c.Markup = _getValueFunc(n));
			else
				_cellRenderer.LambdaSetters.Add ((c, n) => c.Text = _getValueFunc(n));
		}

		public TextRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{
			
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

		public TextRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public TextRendererMapping<TNode> Editable(bool on=true)
		{
			_cellRenderer.Editable = on;
			return this;
		}

		public TextRendererMapping<TNode> Background(string color)
		{
			_cellRenderer.Background = color;
			return this;
		}

		public TextRendererMapping<TNode> WrapMode(Pango.WrapMode mode)
		{
			_cellRenderer.WrapMode = mode;
			return this;
		}

		public TextRendererMapping<TNode> WrapWidth(int width)
		{
			_cellRenderer.WrapWidth = width;
			return this;
		}

		public TextRendererMapping<TNode> WidthChars(int widthChars)
		{
			_cellRenderer.WidthChars = widthChars;
			return this;
		}

		public TextRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public TextRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		public TextRendererMapping<TNode> SearchHighlight(bool on=true)
		{
			_cellRenderer.SearchHighlight = on;
			return this;
		}

		public TextRendererMapping<TNode> Sensitive(bool on=true)
		{
			_cellRenderer.Sensitive = on;
			return this;
		}

		public TextRendererMapping<TNode> AddSetter(Action<NodeCellRendererText<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		public TextRendererMapping<TNode> EditingStartedEvent (Gtk.EditingStartedHandler handler)
		{
			_cellRenderer.EditingStarted += handler;
			AddHandler(handler);
			return this;
		}

		public TextRendererMapping<TNode> EditedEvent (Gtk.EditedHandler handler)
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
				
				_getValueFunc = null;
				base.Dispose();
			}
		}
	}
}

