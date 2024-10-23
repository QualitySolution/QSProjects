using Autofac;
using QS.Dialog;
using QS.ViewModels.Extension;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QS.Navigation;

public class AvaloniaNavigationManager : NavigationManagerBase, INavigationManager {

	IPage? currentPage;
	public IPage? CurrentPage
	{
		get => currentPage;
		protected set => this.RaiseAndSetIfChanged(ref currentPage, value);
	}

	public ObservableCollection<IAvaloniaPage> Pages { get; protected set; } = [];

	// ридонли не риоднли - pohui
	AvaloniaPageTabFactory tabFactory;
	AvaloniaPageWindowFactory windowFactory;

	public AvaloniaNavigationManager(IInteractiveMessage interactive,
		AvaloniaPageWindowFactory windowFactory,
		AvaloniaPageTabFactory tabFactory,
		IPageHashGenerator? hashGenerator = null)
		: base(interactive, hashGenerator)
	{
		this.tabFactory = tabFactory;
		this.windowFactory = windowFactory;
	}

	public bool AskClosePage(IPage page, CloseSource source = CloseSource.External) {
		Pages.Remove((IAvaloniaPage)page);
		CurrentPage = Pages.Count > 0 ? Pages[0] : null;
		return true;
	}

	// View сама переключает CurrentPage, этот метод для внутреннего переключения
	public override void SwitchOn(IPage page) {
		if(!Pages.Contains((IAvaloniaPage)page))
			OpenPage(null, page);
		else
			CurrentPage = page;
	}

	protected override IViewModelsPageFactory GetPageFactory<TViewModel>() {
		if(typeof(TViewModel).IsAssignableTo<IWindowDialogSettings>())
			return windowFactory;
		else
			return tabFactory;
	}

	protected override void OpenPage(IPage masterPage, IPage page) {
		Pages.Add((IAvaloniaPage)page);
		CurrentPage = page;
	}

	protected override void OpenSlavePage(IPage masterPage, IPage page) {
		throw new NotImplementedException();
	}

	public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
		AskClosePage(page);
	}
}
