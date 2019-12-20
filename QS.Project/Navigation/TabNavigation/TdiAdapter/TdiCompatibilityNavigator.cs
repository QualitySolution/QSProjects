using System;
using System.Collections.Generic;
using QS.Tdi;
using QS.ViewModels;
using System.Linq;
using QS.Services;
using QS.Dialog;

namespace QS.Navigation.TabNavigation.TdiAdapter
{
	public class TdiCompatibilityNavigator : ITdiCompatibilityNavigation, ITdiTabParent
	{
		private readonly INavigationManager navigationManager;
		private readonly IInteractiveService interactiveService;

		public TdiCompatibilityNavigator(INavigationManager navigationManager, IInteractiveService interactiveService)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		#region INavigationManager implementation

		public bool AskClosePage(IPage page)
		{
			return navigationManager.AskClosePage(page);
		}

		public IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			return navigationManager.FindPage(viewModel);
		}

		public IPage FindPage(string hashName)
		{
			return navigationManager.FindPage(hashName);
		}

		public void ForceClosePage(IPage page)
		{
			navigationManager.ForceClosePage(page);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModel<TViewModel>(master, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModel<TViewModel, TCtorArg1>(master, arg1, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(master, arg1, arg2, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(master, arg1, arg2, arg3, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModelNamedArgs<TViewModel>(master, ctorArgs, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage OpenViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None)
		{
			var page = navigationManager.OpenViewModelNamedArgs(viewModelType, master, ctorArgs, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var page = navigationManager.OpenViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public IPage OpenViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None)
		{
			var page = navigationManager.OpenViewModelTypedArgs(viewModelType, master, ctorTypes, ctorValues, options);
			page.ViewModel.NavigationManager = this;
			return page;
		}

		public void SwitchOn(IPage page)
		{
			navigationManager.SwitchOn(page);
		}

		#endregion INavigationManager implementation

		#region ITdiCompatibilityNavigation implementation

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(ITdiTab master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, new Type[] { }, new object[] { }, options);
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			TdiTabViewModelAdapter masterAdapter = GetAdapter(master);
			return navigationManager.OpenViewModelTypedArgs<TViewModel>(masterAdapter, ctorTypes, ctorValues, options);
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			return OpenTdiTab<TTab>(masterTab, new Type[] { }, new object[] { });
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			return OpenTdiTab<TTab>(masterTab, new Type[] { typeof(TCtorArg1) }, new object[] { arg1 });
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			ITdiTab tab = CreateTab(typeof(TTab));
			if(options.HasFlag(OpenPageOptions.IgnoreHash)) {
				if(options.HasFlag(OpenPageOptions.AsSlave)) {
					return AddSlaveTabPage(tab, masterTab);
				} else {
					return AddTabPage(tab, masterTab);
				}
			} else {
				if(options.HasFlag(OpenPageOptions.AsSlave)) {
					return OpenSlaveTabPage(tab, masterTab);
				} else {
					return OpenTabPage(tab, masterTab);
				}
			}
		}

		#endregion ITdiCompatibilityNavigation implementation

		#region ITdiTabParent implementation

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			AddSlaveTabPage(slaveTab, masterTab);
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool canSlided = true)
		{
			AddTabPage(tab, afterTab);
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			var page = FindPage(tab);
			if(page.ChildPages.Any()) {
				string message = page.ChildPages.Count() > 1 ? "Сначала надо закрыть подчиненные вкладки." : "Сначала надо закрыть подчиненную вкладку.";
				interactiveService.InteractiveMessage.ShowMessage(ImportanceLevel.Warning, message, "Внимание!");
				return false;
			}

			return true;
		}

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			TdiTabViewModelAdapter tdiAdapter = navigationManager.FindPage(hashName)?.ViewModel as TdiTabViewModelAdapter;
			return tdiAdapter?.Tab;
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			var page = FindPage(tab);
			if(page == null) {
				return;
			}
			navigationManager.SwitchOn(page);
		}

		public bool AskToCloseTab(ITdiTab tab)
		{
			var page = FindPage(tab);
			if(page == null) {
				return false;
			}
			return navigationManager.AskClosePage(page);
		}

		public void ForceCloseTab(ITdiTab tab)
		{
			var page = FindPage(tab);
			if(page == null) {
				return;
			}
			navigationManager.ForceClosePage(page);
		}

		public ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null)
		{
			ITdiTab tab = newTabFunc();
			OpenTabPage(tab, afterTab);
			return tab;
		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true)
		{
			ITdiTab tab = newTabFunc();
			OpenTabPage(tab, afterTab);
			return tab;
		}

		public ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab
		{
			ITdiPage page = OpenTdiTab<TTab>(afterTab);
			return page.TdiTab;
		}

		public ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			ITdiPage page = OpenTdiTab<TTab>(afterTab, new Type[] { typeof(TArg1) }, new object[] { arg1 });
			return page.TdiTab;
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			ITdiPage page = OpenTdiTab<TTab>(afterTab, new Type[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 });
			return page.TdiTab;
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			ITdiPage page = OpenTdiTab<TTab>(afterTab, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 });
			return page.TdiTab;
		}

		#endregion ITdiTabParent implementation

		#region Поиск

		private IPage FindPage(ITdiTab tab)
		{
			TdiTabViewModelAdapter tdiAdapter = GetAdapter(tab);
			if(tdiAdapter == null) {
				return null;
			}

			return navigationManager.FindPage(tdiAdapter);
		}

		#endregion Поиск

		#region Создание Tdi вкладок

		private ITdiTab CreateTab(Type tabType)
		{
			return CreateTab(tabType, new object[] { });
		}

		private ITdiTab CreateTab(Type tabType, params object[] args)
		{
			ITdiTab tab = (ITdiTab)Activator.CreateInstance(tabType, args);
			tab.TabParent = this;
			return tab;
		}

		#endregion Создание Tdi вкладок

		#region Работа с адаптерами

		List<TdiTabViewModelAdapter> adapters = new List<TdiTabViewModelAdapter>();

		private void AddAdapter(TdiTabViewModelAdapter tdiAdapter)
		{
			if(adapters.Contains(tdiAdapter)) {
				return;
			}
			adapters.Add(tdiAdapter);
		}

		private void RemoveAdapter(TdiTabViewModelAdapter tdiAdapter)
		{
			if(adapters.Contains(tdiAdapter)) {
				adapters.Remove(tdiAdapter);
			}
		}

		private TdiTabViewModelAdapter GetAdapter(ITdiTab tab)
		{
			return adapters.FirstOrDefault(x => x.Tab == tab);
		}

		#endregion Работа с адаптерами

		#region Создание, открытие Tdi вкладок

		private ITdiPage AddTabPage(ITdiTab tab, ITdiTab afterTab = null)
		{
			ViewModelBase masterViewModel = (afterTab as ViewModelBase) ?? GetAdapter(afterTab);
			IPage<TdiTabViewModelAdapter> page = navigationManager.OpenViewModel<TdiTabViewModelAdapter, ITdiTab, IInteractiveService>(masterViewModel, tab, interactiveService, OpenPageOptions.IgnoreHash);
			SubscribeRemoveAdapterOnCLosePage(page);
			AddAdapter(page.ViewModel);
			page.ViewModel.NavigationManager = this;
			return new TdiPageProxy(page);
		}

		private ITdiPage AddSlaveTabPage(ITdiTab slaveTab, ITdiTab masterTab = null)
		{
			ViewModelBase masterViewModel = (slaveTab as ViewModelBase) ?? GetAdapter(masterTab);
			IPage<TdiTabViewModelAdapter> page = navigationManager.OpenViewModel<TdiTabViewModelAdapter, ITdiTab, IInteractiveService>(masterViewModel, slaveTab, interactiveService, OpenPageOptions.AsSlave & OpenPageOptions.IgnoreHash);
			SubscribeRemoveAdapterOnCLosePage(page);
			AddAdapter(page.ViewModel);
			page.ViewModel.NavigationManager = this;
			return new TdiPageProxy(page);
		}

		private ITdiPage OpenTabPage(ITdiTab tab, ITdiTab afterTab = null, string hash = null)
		{
			ViewModelBase masterViewModel = (afterTab as ViewModelBase) ?? GetAdapter(afterTab);
			IPage<TdiTabViewModelAdapter> page = navigationManager.OpenViewModel<TdiTabViewModelAdapter, ITdiTab, IInteractiveService>(masterViewModel, tab, interactiveService);
			SubscribeRemoveAdapterOnCLosePage(page);
			AddAdapter(page.ViewModel);
			page.ViewModel.NavigationManager = this;
			return new TdiPageProxy(page);
		}

		private ITdiPage OpenSlaveTabPage(ITdiTab slaveTab, ITdiTab masterTab = null)
		{
			ViewModelBase masterViewModel = (masterTab as ViewModelBase) ?? GetAdapter(masterTab);
			IPage<TdiTabViewModelAdapter> page = navigationManager.OpenViewModel<TdiTabViewModelAdapter, ITdiTab, IInteractiveService>(masterViewModel, slaveTab, interactiveService, OpenPageOptions.AsSlave);
			SubscribeRemoveAdapterOnCLosePage(page);
			AddAdapter(page.ViewModel);
			page.ViewModel.NavigationManager = this;
			return new TdiPageProxy(page);
		}

		private void SubscribeRemoveAdapterOnCLosePage(IPage<TdiTabViewModelAdapter> page)
		{
			page.PageClosed += (sender, e) => RemoveAdapter(page.ViewModel);
		}

		#endregion Создание, открытие Tdi вкладок
	}
}
