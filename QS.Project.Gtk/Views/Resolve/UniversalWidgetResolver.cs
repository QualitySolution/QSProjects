using System;
using Gtk;
using QS.Navigation.GtkUI;
using QS.ViewModels;
using QS.Dialog.GtkUI;
using QS.Tdi.Gtk;
using QS.Tdi;
using System.Collections.Generic;
using QS.Project.Journal;
using QS.Journal.GtkUI;

namespace QS.Views.Resolve
{
	public class UniversalWidgetResolver : IWidgetResolver, IFilterWidgetResolver, ITDIWidgetResolver, IGtkViewResolver
	{
		private Dictionary<Type, Type> viewModelViews = new Dictionary<Type, Type>();
		private Dictionary<Type, Type> baseViewModelViews = new Dictionary<Type, Type>();

		public UniversalWidgetResolver()
		{
			RegisterViewForViewModel<JournalViewModelBase, JournalView>();
		}

		#region IWidgetResolver implementation

		public virtual Widget Resolve(ViewModelBase viewModel)
		{
			Type viewModelType = viewModel.GetType();
			Type viewType = Resolve(viewModelType);

			var viewCtorInfo = viewType.GetConstructor(new[] { viewModelType });
			Widget widget = (Widget)viewCtorInfo.Invoke(new object[] { viewModel });

			return widget;
		}

		#endregion IWidgetResolver implementation

		#region IFilterWidgetResolver implementation

		public virtual Widget Resolve(RepresentationModel.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}

			if(filter is ViewModelBase) {
				return Resolve((ViewModelBase)filter);
			}

			throw new GtkViewResolveException($"Невозможно определить View для типа {filter.GetType().FullName}");
		}

		public virtual Widget Resolve(Project.Journal.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}

			if(filter is ViewModelBase) {
				return Resolve((ViewModelBase)filter);
			}

			throw new GtkViewResolveException($"Невозможно определить View для типа {filter.GetType().FullName}");
		}

		#endregion IFilterWidgetResolver implementation

		#region ITDIWidgetResolver implementation

		public virtual Widget Resolve(ITdiTab tab)
		{
			if(tab == null) {
				throw new ArgumentNullException(nameof(tab));
			}

			if(tab is Widget) {
				return (Widget)tab;
			}

			if(tab is ViewModelBase) {
				return Resolve((ViewModelBase)tab);
			}

			throw new GtkViewResolveException($"Невозможно определить View для типа {tab.GetType().FullName}");
		}

		#endregion ITDIWidgetResolver implementation

		#region IGtkViewResolver implementation

		public virtual Widget Resolve(DialogViewModelBase viewModel)
		{
			if(viewModel == null) {
				throw new ArgumentNullException(nameof(viewModel));
			}

			return Resolve((ViewModelBase)viewModel);
		}

		#endregion IGtkViewResolver implementation

		private Type ResolveViewTypeByBaseType(Type viewModelType)
		{
			Type baseType = viewModelType;
			do {
				baseType = baseType.BaseType;
				if(baseViewModelViews.ContainsKey(baseType)) {
					return baseViewModelViews[baseType];
				}
			} while(baseType != typeof(ViewModelBase));
			return null;
		}

		public virtual Type Resolve(Type viewModelType)
		{
			if(viewModelType == null) {
				throw new ArgumentNullException(nameof(viewModelType));
			}

			if(viewModelViews.ContainsKey(viewModelType)) {
				return viewModelViews[viewModelType];
			}

			Type viewType = ResolveViewTypeByBaseType(viewModelType);
			if(viewType == null) {
				throw new GtkViewResolveException($"Не настроено сопоставление View для {viewModelType.FullName}");
			} else {
				return viewType;
			}
		}

		public virtual UniversalWidgetResolver RegisterViewForViewModel<TViewModel, TView>()
			where TViewModel : ViewModelBase
			where TView : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TView);
			if(viewModelViews.ContainsKey(viewModelType)) {
				viewModelViews[viewModelType] = widgetType;
			}
			viewModelViews.Add(viewModelType, widgetType);

			return this;
		}

		public virtual UniversalWidgetResolver RegisterViewForBaseViewModel<TViewModel, TView>()
			where TViewModel : ViewModelBase
			where TView : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TView);
			if(baseViewModelViews.ContainsKey(viewModelType)) {
				baseViewModelViews[viewModelType] = widgetType;
			}
			baseViewModelViews.Add(viewModelType, widgetType);

			return this;
		}
	}
}
