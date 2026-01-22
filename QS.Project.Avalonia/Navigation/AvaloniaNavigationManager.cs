using Autofac;
using QS.Dialog;
using QS.ViewModels.Extension;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

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
	readonly IAvaloniaViewResolver viewResolver;

	public AvaloniaNavigationManager(IInteractiveMessage interactive,
		AvaloniaPageWindowFactory windowFactory,
		AvaloniaPageTabFactory tabFactory,
		IAvaloniaViewResolver viewResolver,
		IPageHashGenerator? hashGenerator = null)
		: base(interactive, hashGenerator)
	{
		this.tabFactory = tabFactory;
		this.windowFactory = windowFactory;
		this.viewResolver = viewResolver ?? throw new ArgumentNullException(nameof(viewResolver));
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
		page.ViewModel.NavigationManager = this;
		pages.Add(page);
		
		var avaloniaPage = (IAvaloniaPage)page;
		avaloniaPage.View = viewResolver.Resolve(page.ViewModel);
		if(avaloniaPage.View == null)
			throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");
		
		Pages.Add(avaloniaPage);
		CurrentPage = page;
	}

	protected override void OpenSlavePage(IPage masterPage, IPage page) {
		throw new NotImplementedException();
	}

	public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
		AskClosePage(page);
	}
}
