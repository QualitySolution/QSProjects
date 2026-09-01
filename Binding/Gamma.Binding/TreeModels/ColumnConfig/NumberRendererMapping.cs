using System;
using System.Linq.Expressions;
using Gamma.Utilities;
using Gamma.GtkWidgets.Cells;
using Gtk;
using Gamma.Binding;

namespace Gamma.ColumnConfig
{
	public class NumberRendererMapping<TNode> : RendererMappingBase<NodeCellRendererSpin<TNode>, TNode>, ICustomRendererMapping
	{
		private readonly NodeCellRendererSpin<TNode> _cellRenderer = new NodeCellRendererSpin<TNode>();

		public NumberRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> getDataExp)
			: base(column)
		{
			_cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo<TNode> (getDataExp);
			var getter = getDataExp.Compile();
			_cellRenderer.LambdaSetters.Add ((c, n) => c.Text = String.Format ("{0:" + String.Format ("F{0}", c.Digits) + "}", getter (n)));
		}

		public NumberRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> getDataExp, IValueConverter converter)
			: base(column)
		{
			_cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo<TNode> (getDataExp);
			_cellRenderer.EditingValueConverter = converter;
			var getter = getDataExp.Compile();
			_cellRenderer.LambdaSetters.Add ((c, n) => 
				c.Text = String.Format ("{0:" + String.Format ("F{0}", c.Digits) + "}",
			                            c.EditingValueConverter.Convert (getter (n), typeof(double), null, null)));
		}
		
		public NumberRendererMapping(
			ColumnMapping<TNode> column,
			Expression<Func<TNode, object>> getDataExp,
			EditedHandler editedHandler,
			bool withThousandsSeparator) : base(column)
		{
			var getter = getDataExp.Compile();
			_cellRenderer.Edited += editedHandler;
			AddHandler(editedHandler);
			Custom = true;

			var numberFormat = withThousandsSeparator ? "N" : "F";
			_cellRenderer.LambdaSetters.Add((c, n) =>
				c.Text = string.Format("{0:" + $"{numberFormat}{c.Digits}" + "}", getter(n)));
		}

		public NumberRendererMapping (ColumnMapping<TNode> column)
			: base(column)
		{

		}
		
		public bool Custom { get; }

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer ()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererSpin<TNode>, TNode> commonSet)
		{
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		public NumberRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public NumberRendererMapping<TNode> AddSetter(Action<NodeCellRendererSpin<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		#region Fluent

		public NumberRendererMapping<TNode> Digits(uint digits)
		{
			_cellRenderer.Digits = digits;
			return this;
		}

		public NumberRendererMapping<TNode> Background(string color)
		{
			_cellRenderer.Background = color;
			return this;
		}

		public NumberRendererMapping<TNode> Adjustment(Adjustment adjustment)
		{
			_cellRenderer.Adjustment = adjustment;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public NumberRendererMapping<TNode> Editing (bool on = true)
		{
			_cellRenderer.Editable = on;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public NumberRendererMapping<TNode> Editing(Func<TNode, bool> editingFunc) {
			_cellRenderer.LambdaSetters.Add((c, n) => c.Editable = editingFunc(n));
			return this;
		}

		public NumberRendererMapping<TNode> Editing (Adjustment adjustment, bool on = true)
		{
			_cellRenderer.Adjustment = adjustment;
			_cellRenderer.Editable = on;
			return this;
		}

		public NumberRendererMapping<TNode> WidthChars(int widthChars)
		{
			_cellRenderer.WidthChars = widthChars;
			return this;
		}

		public NumberRendererMapping<TNode> EnterToNextCell()
		{
			_cellRenderer.IsEnterToNextCell = true;
			return this;
		}

		public NumberRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public NumberRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		public NumberRendererMapping<TNode> EditedEvent(EditedHandler handler)
		{
			_cellRenderer.Edited += handler;
			AddHandler(handler);
			return this;
		}
		
		public NumberRendererMapping<TNode> EditingStartedEvent(EditingStartedHandler handler)
		{
			_cellRenderer.EditingStarted += handler;
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
