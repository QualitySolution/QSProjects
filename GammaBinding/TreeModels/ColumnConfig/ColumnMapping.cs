using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Utilities;

namespace Gamma.ColumnConfig
{
	public class ColumnMapping<TNode> : IColumnMapping
	{
		FluentColumnsConfig<TNode> myConfig;

		public string Title { get; set;}

		public string DataPropertyName { get; set;}

		public bool IsEditable { get; set;}

		private readonly List<IRendererMappingGeneric<TNode>> Renderers = new List<IRendererMappingGeneric<TNode>> ();

		public IEnumerable<IRendererMapping> ConfiguredRenderers {
			get { return Renderers;	}
		}

		public IEnumerable<IRendererMappingGeneric<TNode>> ConfiguredRenderersGeneric {
			get { return Renderers;	}
		}

		public ColumnMapping (FluentColumnsConfig<TNode> parentConfig, string title)
		{
			this.myConfig = parentConfig;
			Title = title;
		}

		/// <summary>
		/// Set only if it simple column mapping, else using sets from Render.
		/// </summary>
		/// <param name="propertyRefExpr">Property Name expr.</param>
		/// <typeparam name="TVMNode">Node type</typeparam>
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
				throw new NotSupportedException (String.Format ("Type {0} isn't supports.", properyType));
			}
			return this;
		}

		public ColumnMapping<TNode> SetDataProperty (Expression<Func<TNode, string>> propertyRefExpr)
		{
			//DataPropertyName = PropertyUtil.GetName (propertyRefExpr);
			AddTextRenderer (propertyRefExpr);
			return this;
		}

		public ColumnMapping<TNode> SetDataProperty (Expression<Func<TNode, bool>> propertyRefExpr)
		{
			//DataPropertyName = PropertyUtil.GetName (propertyRefExpr);
			AddToggleRenderer (propertyRefExpr);
			return this;
		}

		public ColumnMapping<TNode> Editing ()
		{
			IsEditable = true;
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

		#region Renderers

		public TextRendererMapping<TNode> AddTextRenderer(Expression<Func<TNode, string>> dataProperty, bool expand = true)
		{
			var render = new TextRendererMapping<TNode> (this, dataProperty);
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

		public ToggleRendererMapping<TNode> AddToggleRenderer(Expression<Func<TNode, bool>> dataProperty, bool expand = true)
		{
			var render = new ToggleRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public EnumRendererMapping<TNode> AddEnumRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			var render = new EnumRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		public ComboRendererMapping<TNode> AddComboRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			var render = new ComboRendererMapping<TNode> (this, dataProperty);
			render.IsExpand = expand;
			Renderers.Add (render);
			return render;
		}

		#endregion
	}
}

