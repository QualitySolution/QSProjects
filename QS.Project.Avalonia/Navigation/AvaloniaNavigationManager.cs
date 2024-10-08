using Autofac;
using QS.Dialog;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace QS.Navigation;

public class AvaloniaNavigationManager : NavigationManagerBase, INavigationManager {
	public IPage? CurrentPage { get; protected set; }

	protected AvaloniaNavigationManager(IInteractiveMessage interactive, IPageHashGenerator hashGenerator = null)
		: base(interactive, hashGenerator) { }

	public bool AskClosePage(IPage page, CloseSource source = CloseSource.External) {
		throw new NotImplementedException();
	}

	public IPage FindPage(DialogViewModelBase viewModel) {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> FindPage<TViewModel>(DialogViewModelBase viewModel) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModel<TViewModel>(DialogViewModelBase master, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(DialogViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<TViewModel> configureViewModel = null, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public override void SwitchOn(IPage page) {
		CurrentPage = page;

		throw new NotImplementedException();
	}

	protected override IViewModelsPageFactory GetPageFactory<TViewModel>() {
		throw new NotImplementedException();
	}

	protected override void OpenPage(IPage masterPage, IPage page) {
		CurrentPage = page;

		throw new NotImplementedException();
	}

	protected override void OpenSlavePage(IPage masterPage, IPage page) {
		throw new NotImplementedException();
	}
}
