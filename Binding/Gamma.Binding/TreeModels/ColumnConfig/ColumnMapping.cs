﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Binding;
using Gamma.Utilities;
using Gdk;
using Gtk;

namespace Gamma.ColumnConfig
{
	public class ColumnMapping<TNode> : IColumnMapping
	{
		FluentColumnsConfig<TNode> myConfig;

		#region Propeties

		public TreeViewColumn TreeViewColumn { get; private set;}

		public string Title { get{ return TreeViewColumn.Title;	}}

		public string DataPropertyName { get; set;}

		public bool IsEnterToNextCell { get; set;}

		public object tag { get; set; }

		private readonly List<IRendererMappingGeneric<TNode>> Renderers = new List<IRendererMappingGeneric<TNode>> ();

		public IEnumerable<IRendererMapping> ConfiguredRenderers {
			get { return Renderers;	}
		}

		public IEnumerable<IRendererMappingGeneric<TNode>> ConfiguredRenderersGeneric {
			get { return Renderers;	}
		}


		#endregion

		public ColumnMapping (FluentColumnsConfig<TNode> parentConfig, string title)
		{
			TreeViewColumn = new TreeViewColumn();
			this.myConfig = parentConfig;
			TreeViewColumn.Title = title;
		}

		#region FluentConfig

		/// <summary>
		/// Set only if it simple column mapping, else using sets from Render.
		/// </summary>
		/// <param name="propertyRefExpr">Property Name expr.</param>
		/// <typeparam name="TVMNode">Node type</typeparam>
		[Obsolete("Используете вызов Add{*}Renderer напрямую.")]
		public ColumnMapping<TNode> SetDataProperty (Expression<Func<TNode, object>> propertyRefExpr)
		{
			DataPropertyName = PropertyUtil.GetName (propertyRefExpr);
			Type properyType = typeof(TNode).GetProperty (DataPropertyName).PropertyType;
			if(TypeUtil.IsNumeric(properyType))
			{
				AddNumericRenderer (propertyRefExpr);
			}
			else 
			{
				throw new NotSupportedException (String.Format ("Type {0} is not supported.", properyType));
			}
			return this;
		}

		[Obsolete("Используете вызов AddTextRenderer напрямую.")]
		public ColumnMapping<TNode> SetDataProperty (Expression<Func<TNode, string>> propertyRefExpr)
		{
			//DataPropertyName = PropertyUtil.GetName (propertyRefExpr);
			AddTextRenderer (propertyRefExpr);
			return this;
		}

		public ColumnMapping<TNode> SetTag(object tag)
		{
			this.tag = tag;
			return this;
		}

		[Obsolete("Используете вызов AddToggleRenderer напрямую.")]
		public ColumnMapping<TNode> SetDataProperty (Expression<Func<TNode, bool>> propertyRefExpr)
		{
			//DataPropertyName = PropertyUtil.GetName (propertyRefExpr);
			AddToggleRenderer (propertyRefExpr);
			return this;
		}

		public ColumnMapping<TNode> HeaderAlignment (float x)
		{
			TreeViewColumn.Alignment = x;
			return this;
		}

		public ColumnMapping<TNode> MinWidth(int pixels)
		{
			TreeViewColumn.MinWidth = pixels;
			return this;
		}

		public ColumnMapping<TNode> Resizable (bool resizeble = true)
		{
			TreeViewColumn.Resizable = resizeble;
			return this;
		}

		public ColumnMapping<TNode> Tag(object tag)
		{
			this.tag = tag;
			return this;
		}

		public ColumnMapping<TNode> Visible(bool visible)
		{
			TreeViewColumn.Visible = visible;
			return this;
		}

		public ColumnMapping<TNode> EnterToNextCell ()
		{
			IsEnterToNextCell = true;
			return this;
		}

		public ColumnMapping<TNode> ClickedEvent(EventHandler clicked)
		{
			TreeViewColumn.Clicked += clicked;
			return this;
		}

		/// <summary>
		/// Позволяет установить функцию получения текста всплывающей подсказки для ячеек колонки.
		/// </summary>
		public ColumnMapping<TNode> ToolTipText(Func<TNode, string> tooltipText)
		{
			cellTooltipTextFunc = tooltipText;
			return this;
		}

		public ColumnMapping<TNode> AddColumn(string title)
		{
			return myConfig.AddColumn (title);
		}

		public RowMapping<TNode> RowCells()
		{
			return myConfig.RowCells ();
		}

		public IColumnsConfig Finish()
		{
			return myConfig.Finish ();
		}

		#endregion

		#region Renderers

		public TextRendererMapping<TNode> AddTextRenderer(Expression<Func<TNode, string>> dataProperty, bool expand = true, bool useMarkup = false)
		{
			var render = new TextRendererMapping<TNode> (this, dataProperty, useMarkup);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public TextRendererMapping<TNode> AddTextRenderer()
		{
			var render = new TextRendererMapping<TNode> (this);
			Renderers.Add (render);
			return render;
		}

		public ProgressRendererMapping<TNode> AddProgressRenderer(Expression<Func<TNode, int>> dataProperty, bool expand = true)
		{
			var render = new ProgressRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			var render = new NumberRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, IValueConverter converter, bool expand = true)
		{
			var render = new NumberRendererMapping<TNode> (this, dataProperty, converter);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public ToggleRendererMapping<TNode> AddToggleRenderer(Expression<Func<TNode, bool>> dataProperty, bool expand = true)
		{
			var render = new ToggleRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public EnumRendererMapping<TNode, TItem> AddEnumRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true, Enum [] excludeItems = null) where TItem : struct, IConvertible
		{
			var render = new EnumRendererMapping<TNode, TItem> (this, dataProperty, excludeItems);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public ComboRendererMapping<TNode, TItem> AddComboRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true)
		{
			var render = new ComboRendererMapping<TNode, TItem> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public PixbufRendererMapping<TNode> AddPixbufRenderer(Expression<Func<TNode, Pixbuf>> dataProperty, bool expand = true)
		{
			var render = new PixbufRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		#endregion

		#region ToolTip

		private Func<TNode, string> cellTooltipTextFunc;

		public bool HasToolTip => cellTooltipTextFunc != null;

		public string GetTooltipText(object node) => cellTooltipTextFunc((TNode)node);

		#endregion
	}
}

