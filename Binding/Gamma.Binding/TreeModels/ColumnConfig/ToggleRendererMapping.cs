using System;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class ToggleRendererMapping<TNode> : RendererMappingBase<NodeCellRendererToggle<TNode>, TNode>
	{
		private readonly NodeCellRendererToggle<TNode> _cellRenderer = new NodeCellRendererToggle<TNode>();

		public ToggleRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, bool>> getDataExp)
			: base(column)
		{
			_cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo (getDataExp);
			var getter = getDataExp.Compile();
			_cellRenderer.LambdaSetters.Add ((c, n) => c.Active = getter(n));
		}

		public ToggleRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererToggle<TNode>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		#region FluentConfig

		public ToggleRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public ToggleRendererMapping<TNode> AddSetter(Action<NodeCellRendererToggle<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}
			
		public ToggleRendererMapping<TNode> Editing (bool on = true)
		{
			_cellRenderer.Activatable = on;
			return this;
		}

		public ToggleRendererMapping<TNode> Radio(bool on = true)
		{
			_cellRenderer.Radio = on;
			return this;
		}

		public ToggleRendererMapping<TNode> ToggledEvent (ToggledHandler handler)
		{
			_cellRenderer.Toggled += handler;
			AddHandler(handler);
			return this;
		}

		public ToggleRendererMapping<TNode> ChangeSetProperty(PropertyInfo property)
		{
			_cellRenderer.DataPropertyInfo = property;
			return this;
		}

		public ToggleRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public ToggleRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
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

