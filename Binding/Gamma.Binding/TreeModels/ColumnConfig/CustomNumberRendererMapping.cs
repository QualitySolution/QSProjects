using System;
using System.Linq.Expressions;
using Gamma.Binding;
using Gamma.GtkWidgets.Cells;
using Gtk;

namespace Gamma.ColumnConfig
{
    /// <summary>
    /// Маппинг свойста для чтения и установки значения через EditedHandler
    /// Например, нам надо при установке значения запускать определенную логику
    /// и чтобы два раза не устанавливать одно и тоже значение через сеттер и EditedHandler,
    /// используем этот класс
    /// </summary>
    /// <typeparam name="TNode">Нода, свойство котрой маппится</typeparam>
	public class CustomNumberRendererMapping<TNode> : RendererMappingBase<NodeCellRendererSpin<TNode>, TNode>
	{
		private readonly NodeCellRendererSpin<TNode> _cellRenderer = new NodeCellRendererSpin<TNode>();
		
		public CustomNumberRendererMapping(
			ColumnMapping<TNode> column,
			Expression<Func<TNode, object>> getDataExp,
			EditedHandler editedHandler) : base(column)
		{
			var getter = ConfigureCustomMapping(getDataExp, editedHandler);
			_cellRenderer.LambdaSetters.Add((c, n) => c.Text = String.Format("{0:" + $"F{c.Digits}" + "}", getter(n)));
		}

		public CustomNumberRendererMapping(
			ColumnMapping<TNode> column,
			Expression<Func<TNode, object>> getDataExp,
			EditedHandler editedHandler,
			IValueConverter converter) : base(column)
		{
			_cellRenderer.EditingValueConverter = converter;
			var getter = ConfigureCustomMapping(getDataExp, editedHandler);
			_cellRenderer.LambdaSetters.Add((c, n) => 
				c.Text = String.Format ("{0:" + $"F{c.Digits}" + "}",
					c.EditingValueConverter.Convert(getter(n), typeof(double), null, null)));
		}

		#region implemented abstract members of RendererMappingBase

		public override INodeCellRenderer GetRenderer()
		{
			return _cellRenderer;
		}

		protected override void SetSetterSilent(Action<NodeCellRendererSpin<TNode>, TNode> commonSet) {
			_cellRenderer.LambdaSetters.Insert(0, commonSet);
		}

		#endregion

		public CustomNumberRendererMapping<TNode> SetTag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public CustomNumberRendererMapping<TNode> AddSetter(Action<NodeCellRendererSpin<TNode>, TNode> setter)
		{
			_cellRenderer.LambdaSetters.Add (setter);
			return this;
		}

		#region Fluent

		public CustomNumberRendererMapping<TNode> Digits(uint digits)
		{
			_cellRenderer.Digits = digits;
			return this;
		}

		public CustomNumberRendererMapping<TNode> Background(string color)
		{
			_cellRenderer.Background = color;
			return this;
		}

		public CustomNumberRendererMapping<TNode> Adjustment(Adjustment adjustment)
		{
			_cellRenderer.Adjustment = adjustment;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public CustomNumberRendererMapping<TNode> Editing(bool on = true)
		{
			_cellRenderer.Editable = on;
			return this;
		}

		/// <summary>
		/// If you enable editing don't forget add Adjustment
		/// </summary>
		public CustomNumberRendererMapping<TNode> Editing(Func<TNode, bool> editingFunc) {
			_cellRenderer.LambdaSetters.Add((c, n) => c.Editable = editingFunc(n));
			return this;
		}

		public CustomNumberRendererMapping<TNode> Editing(Adjustment adjustment)
		{
			_cellRenderer.Adjustment = adjustment;
			_cellRenderer.Editable = true;
			return this;
		}

		public CustomNumberRendererMapping<TNode> WidthChars(int widthChars)
		{
			_cellRenderer.WidthChars = widthChars;
			return this;
		}

		public CustomNumberRendererMapping<TNode> EnterToNextCell()
		{
			_cellRenderer.IsEnterToNextCell = true;
			return this;
		}

		public CustomNumberRendererMapping<TNode> XAlign(float alignment)
		{
			_cellRenderer.Xalign = alignment;
			return this;
		}
		
		public CustomNumberRendererMapping<TNode> YAlign(float alignment)
		{
			_cellRenderer.Yalign = alignment;
			return this;
		}

		#endregion
		
		private Func<TNode, object> ConfigureCustomMapping(Expression<Func<TNode, object>> getDataExp, EditedHandler editedHandler)
		{
			var getter = getDataExp.Compile();
			_cellRenderer.Edited += editedHandler;
			Custom = true;
			return getter;
		}
	}
}

