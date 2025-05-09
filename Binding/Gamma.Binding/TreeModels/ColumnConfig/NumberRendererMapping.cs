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
		private NodeCellRendererSpin<TNode> cellRenderer = new NodeCellRendererSpin<TNode>();
		

		public NumberRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> getDataExp)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo<TNode> (getDataExp);
			var getter = getDataExp.Compile();
			cellRenderer.LambdaSetters.Add ((c, n) => c.Text = String.Format ("{0:" + String.Format ("F{0}", c.Digits) + "}", getter (n)));
		}

		public NumberRendererMapping (ColumnMapping<TNode> column, Expression<Func<TNode, object>> getDataExp, IValueConverter converter)
			: base(column)
		{
			cellRenderer.DataPropertyInfo = PropertyUtil.GetPropertyInfo<TNode> (getDataExp);
			cellRenderer.EditingValueConverter = converter;
			var getter = getDataExp.Compile();
			cellRenderer.LambdaSetters.Add ((c, n) => 
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
			cellRenderer.Edited += editedHandler;
			Custom = true;

			var numberFormat = withThousandsSeparator ? "N" : "F";
			cellRenderer.LambdaSetters.Add((c, n) =>
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
			return cellRenderer;
		}

		protected override void SetSetterSilent (Action<NodeCellRendererSpin<TNode>, TNode> commonSet)
		{
			cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		public NumberRendererMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public NumberRendererMapping<TNode> AddSetter(Action<NodeCellRendererSpin<TNode>, TNode> setter)
		{
			cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		#region Fluent

		public NumberRendererMapping<TNode> Digits(uint digits)
		{
			cellRenderer.Digits = digits;
			return this;
		}

		public NumberRendererMapping<TNode> Background(string color)
		{
			cellRenderer.Background = color;
			return this;
		}

		public NumberRendererMapping<TNode> Adjustment(Adjustment adjustment)
		{
			cellRenderer.Adjustment = adjustment;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public NumberRendererMapping<TNode> Editing (bool on = true)
		{
			cellRenderer.Editable = on;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public NumberRendererMapping<TNode> Editing(Func<TNode, bool> editingFunc) {
			cellRenderer.LambdaSetters.Add((c, n) => c.Editable = editingFunc(n));
			return this;
		}

		public NumberRendererMapping<TNode> Editing (Adjustment adjustment, bool on = true)
		{
			cellRenderer.Adjustment = adjustment;
			cellRenderer.Editable = on;
			return this;
		}

		public NumberRendererMapping<TNode> WidthChars(int widthChars)
		{
			cellRenderer.WidthChars = widthChars;
			return this;
		}

		public NumberRendererMapping<TNode> EnterToNextCell()
		{
			cellRenderer.IsEnterToNextCell = true;
			return this;
		}

		public NumberRendererMapping<TNode> XAlign(float alignment)
		{
			cellRenderer.Xalign = alignment;
			return this;
		}
		
		public NumberRendererMapping<TNode> YAlign(float alignment)
		{
			cellRenderer.Yalign = alignment;
			return this;
		}

		public NumberRendererMapping<TNode> EditedEvent(EditedHandler handler)
		{
			cellRenderer.Edited += handler;
			return this;
		}
		
		public NumberRendererMapping<TNode> EditingStartedEvent(EditingStartedHandler handler)
		{
			cellRenderer.EditingStarted += handler;
			return this;
		}

		#endregion
	}
}

