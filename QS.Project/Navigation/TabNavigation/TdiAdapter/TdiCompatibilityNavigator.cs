using System;
using System.Collections.Generic;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation.TabNavigation.TdiAdapter
{
	public class TdiCompatibilityNavigator : ITdiCompatibilityNavigation, ITdiTabParent
	{
		private readonly INavigationManager navigationManager;

		public TdiCompatibilityNavigator(INavigationManager navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		#region ITdiCompatibilityNavigation implementation

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(ITdiTab master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			throw new NotImplementedException();
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			throw new NotImplementedException();
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		#endregion ITdiCompatibilityNavigation implementation

		#region INavigationManager implementation

		public bool AskClosePage(IPage page)
		{
			return navigationManager.AskClosePage(page);
		}

		public IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			return navigationManager.FindPage(viewModel);
		}

		public void ForceClosePage(IPage page)
		{
			navigationManager.ForceClosePage(page);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModel<TViewModel>(master, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModel<TViewModel, TCtorArg1>(master, arg1, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(master, arg1, arg2, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(master, arg1, arg2, arg3, options);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModelNamedArgs<TViewModel>(master, ctorArgs, options);
		}

		public IPage OpenViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None)
		{
			return navigationManager.OpenViewModelNamedArgs(viewModelType, master, ctorArgs, options);
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return navigationManager.OpenViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, options);
		}

		public IPage OpenViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None)
		{
			return navigationManager.OpenViewModelTypedArgs(viewModelType, master, ctorTypes, ctorValues, options);
		}

		public void SwitchOn(IPage page)
		{
			navigationManager.SwitchOn(page);
		}

		#endregion INavigationManager implementation

		#region ITdiTabParent implementation

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			throw new NotImplementedException();
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool canSlided = true)
		{
			throw new NotImplementedException();
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			throw new NotImplementedException();
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		public bool AskToCloseTab(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		public void ForceCloseTab(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null)
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true)
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			throw new NotImplementedException();
		}

		#endregion ITdiTabParent implementation
	}
}
