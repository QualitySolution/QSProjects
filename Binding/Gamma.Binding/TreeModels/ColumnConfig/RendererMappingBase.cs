using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Gamma.GtkWidgets.Cells;
using Gamma.Binding;
using Gtk;
using Object = GLib.Object;

namespace Gamma.ColumnConfig
{
	public abstract class RendererMappingBase<TCellRenderer, TNode> : IRendererMappingGeneric<TNode>
	{
		ColumnMapping<TNode> myColumn;

		public bool IsExpand { get; set;}

		public object tag { get; set; }

		protected RendererMappingBase (ColumnMapping<TNode> parentColumn)
		{
			myColumn = parentColumn;
		}

		public abstract INodeCellRenderer GetRenderer();
		protected readonly IDictionary<Type, IList<object>> EventHandlers = new Dictionary<Type, IList<object>>();

		protected abstract void SetSetterSilent(Action<TCellRenderer, TNode> commonSet);

		public void SetCommonSetter<TActionCellRenderer>(Action<TActionCellRenderer, TNode> commonSet) where TActionCellRenderer : class
		{
			if(typeof(TActionCellRenderer).IsAssignableFrom (typeof(TCellRenderer)))
			{
				SetSetterSilent ((c, n) => commonSet(c as TActionCellRenderer, n));
			}
		}

		public ColumnMapping<TNode> AddColumn(string title)
		{
			return myColumn.AddColumn (title);
		}

		public RowMapping<TNode> RowCells()
		{
			return myColumn.RowCells ();
		}

		public IColumnsConfig Finish()
		{
			return myColumn.Finish ();
		}
		
		protected void AddHandler(object handler) {
			if(EventHandlers.TryGetValue(handler.GetType(), out IList<object> handlers)) {
				handlers.Add(handler);
			}
			else {
				EventHandlers.Add(handler.GetType(), new List<object> { handler });
			}
		}

		#region Renderers
		public ComboRendererMapping<TNode, TItem> AddComboRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true)
		{
			return myColumn.AddComboRenderer (dataProperty, expand);
		}
		
		public DateRendererMapping<TNode> AddDateRenderer(Expression<Func<TNode, DateTime?>> dataProperty, bool expand = true)
		{
			return myColumn.AddDateRenderer (dataProperty, expand);
		}

		public TimeRendererMapping<TNode> AddTimeRenderer(Expression<Func<TNode, TimeSpan?>> dataProperty, bool expand = true) {
			return myColumn.AddTimeRenderer(dataProperty, expand);
		}

		public EnumRendererMapping<TNode, TItem> AddEnumRenderer<TItem>(Expression<Func<TNode, TItem>> dataProperty, bool expand = true, Enum [] excludeItems = null) where TItem : struct, IConvertible
		{
			return myColumn.AddEnumRenderer (dataProperty, expand, excludeItems);
		}
		
		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, bool expand = true)
		{
			return myColumn.AddNumericRenderer (dataProperty, expand);
		}
		
		public NumberRendererMapping<TNode> AddNumericRenderer(Expression<Func<TNode, object>> dataProperty, IValueConverter converter, bool expand = true)
		{
			return myColumn.AddNumericRenderer (dataProperty, converter, expand);
		}
		
		public NumberRendererMapping<TNode> AddNumericRenderer(
			Expression<Func<TNode, object>> dataProperty,
			EditedHandler editedHandler,
			bool withThousandsSeparator = false,
			bool expand = true)
		{
			return myColumn.AddNumericRenderer(dataProperty, editedHandler, withThousandsSeparator, expand);
		}

		public PixbufRendererMapping<TNode> AddPixbufRenderer (Expression<Func<TNode, Gdk.Pixbuf>> dataProperty, bool expand = true)
		{
			return myColumn.AddPixbufRenderer (dataProperty, expand);
		}
		
		public ProgressRendererMapping<TNode> AddProgressRenderer(Expression<Func<TNode, int>> dataProperty, bool expand = true)
		{
			return myColumn.AddProgressRenderer (dataProperty, expand);
		}

		/// <summary>
		/// Добавляет рендер текстовой ячейки используемый только для отображения.
		/// Если нет необходимости редактировать значение, то рендеру не нужно знать свойство для мапинга, в этом случае ему на вход можно передать сложную лямбду.  
		/// </summary>
		/// <param name="getTextFunc">Функция получения значения для отображения</param>
		public ReadOnlyTextRendererMapping<TNode> AddReadOnlyTextRenderer(Func<TNode, string> getTextFunc, bool expand = true, bool useMarkup = false)
		{
			return myColumn.AddReadOnlyTextRenderer(getTextFunc, expand, useMarkup);
		}

		public TextRendererMapping<TNode> AddTextRenderer(Expression<Func<TNode, string>> dataProperty, bool expand = true, bool useMarkup = false)
		{
			return myColumn.AddTextRenderer (dataProperty, expand, useMarkup);
		}

		public TextRendererMapping<TNode> AddTextRenderer()
		{
			return myColumn.AddTextRenderer ();
		}

		public ToggleRendererMapping<TNode> AddToggleRenderer(Expression<Func<TNode, bool>> dataProperty, bool expand = true)
		{
			return myColumn.AddToggleRenderer (dataProperty, expand);
		}
		#endregion

		public virtual void Dispose() {
			var toggleRefPropertyInfo = typeof(TCellRenderer)
				.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.SingleOrDefault(p => p.Name == "ToggleRef");

			if(toggleRefPropertyInfo != null) {
				var renderer = GetRenderer() as CellRenderer;
				var handle = renderer.Handle;
				
				if(handle == IntPtr.Zero) return;
				
				//TODO сделать удаление объекта из словаря объектов Glib.Object Objects 
				var toggleRef = toggleRefPropertyInfo.GetValue(renderer);

				if(toggleRef != null) {
					
					var gch = toggleRef.GetType()
						.GetField("gch", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
						.GetValue(toggleRef);

					if(!(gch is GCHandle gcHandle)) return;
					if(!gcHandle.IsAllocated) return;
					
					var freeMethodInfo = toggleRef.GetType().GetMethod("Free");

					if(freeMethodInfo != null) {
						freeMethodInfo.Invoke(toggleRef, null);
						GC.SuppressFinalize(renderer);
					}
				}
			}
		}
	}
}
