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
using QS.ViewModels.Dialog;
using System.Linq;

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

		private bool TryResolveViewTypeByBaseType(Type viewModelType, out Type viewType)
		{
			viewType = null;
			Type baseType = viewModelType;
			do {
				baseType = baseType.BaseType;
				if(baseViewModelViews.ContainsKey(baseType)) {
					viewType = baseViewModelViews[baseType];
					return true;
				}
			} while(baseType != typeof(ViewModelBase));
			return false;
		}

		private bool TryResolveViewTypeByInterface(Type viewModelType, out Type viewType)
		{
			viewType = null;
			Type[] interfaceTypes = viewModelType.GetInterfaces();
			var interfaceViewTypes = interfaceTypes.Where(x => viewModelViews.ContainsKey(x)).Select(x => viewModelViews[x]);
			int registeredInterfacesCount = interfaceViewTypes.Count();
			if(registeredInterfacesCount > 1) {
				throw new GtkViewResolveException($"Невозможно точно определить View, так как для типа {viewModelType.FullName} зарегистрировано более одного интерфейса.");
			}
			if(registeredInterfacesCount == 1) {
				viewType = interfaceViewTypes.First();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Разрешает тип View для ViewModel.
		/// Сначала проверяется точное совпадение по типу ViewModel,
		/// если не не удается разрешить, то проверятся по наиболее
		/// близкий базовый класс в структуре наследования, если для
		/// них не удается разрешить, то проверяются сопоставления по 
		/// интерфейсам, должено быть совпадение только по одному
		/// интерфейсу, если найдено несколько сопоставлений по интерфейсу
		/// то кидается <see cref="GtkViewResolveException"/>
		/// </summary>
		/// <param name="viewModelType"></param>
		/// <exception cref="GtkViewResolveException">Возникает при невозможности разрешить тип</exception>
		public virtual Type Resolve(Type viewModelType)
		{
			if(viewModelType == null) {
				throw new ArgumentNullException(nameof(viewModelType));
			}

			if(viewModelViews.ContainsKey(viewModelType)) {
				return viewModelViews[viewModelType];
			}

			Type viewType;

			if(TryResolveViewTypeByBaseType(viewModelType, out viewType)) {
				return viewType;
			}

			if(TryResolveViewTypeByInterface(viewModelType, out viewType)) {
				return viewType;
			}

			throw new GtkViewResolveException($"Не настроено сопоставление View для {viewModelType.FullName}");
		}

		public virtual UniversalWidgetResolver RegisterViewForViewModel<TViewModel, TView>()
			where TViewModel : class
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
