using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QS.Dialog;
using QS.Tdi;
using QS.ViewModels.Extension;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace QS.Navigation;

public class AvaloniaNavigationManager : NavigationManagerBase, INavigationManager {

	IPage? currentPage;
	public IPage? CurrentPage
	{
		get => currentPage;
		set => this.RaiseAndSetIfChanged(ref currentPage, value);
	}

	public ObservableCollection<IAvaloniaPage> Pages { get; protected set; } = [];

	// ридонли не риоднли - pohui
	AvaloniaPageTabFactory tabFactory;
	AvaloniaPageWindowFactory windowFactory;
	readonly IAvaloniaViewResolver viewResolver;
	readonly IInteractiveQuestion? interactiveQuestion;

	public AvaloniaNavigationManager(IInteractiveMessage interactive,
		AvaloniaPageWindowFactory windowFactory,
		AvaloniaPageTabFactory tabFactory,
		IAvaloniaViewResolver viewResolver,
		IPageHashGenerator? hashGenerator = null,
		IInteractiveQuestion? interactiveQuestion = null)
		: base(interactive, hashGenerator) {
		this.tabFactory = tabFactory;
		this.windowFactory = windowFactory;
		this.viewResolver = viewResolver ?? throw new ArgumentNullException(nameof(viewResolver));
		this.interactiveQuestion = interactiveQuestion;
	}

	public bool AskClosePage(IPage page, CloseSource source = CloseSource.External) {
		if(source != CloseSource.WithMasterPage && !CanClosePage(page))
			return false;
		ForceClosePage(page, source);
		return true;
	}

	public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
		if(page is IAvaloniaWindowPage) {
			ClosePage(page, source);
			return;
		}
		var avaloniaPage = (IAvaloniaPage)page;
		bool wasCurrent = CurrentPage == page;
		var master = SlavePages.FirstOrDefault(x => x.SlavePage == page)?.MasterPage;
		int index = Pages.IndexOf(avaloniaPage);
		Pages.Remove(avaloniaPage);
		ClosePage(page, source);
		if(!wasCurrent)
			return;

		// после закрытия подчиненной возвращаемся на хозяйскую, иначе на соседнюю вкладку
		if(master is IAvaloniaPage masterTab && Pages.Contains(masterTab))
			CurrentPage = master;
		else
			CurrentPage = Pages.Count > 0 ? Pages[Math.Max(0, Math.Min(index, Pages.Count - 1))] : null;
	}

	bool CanClosePage(IPage page) {
		var askSave = (page.ViewModel as IAskSaveOnCloseViewModel)?.AskSaveOnClose ?? true;
		if(interactiveQuestion == null || !askSave)
			return true;
		if(!(page.ViewModel is ISaveable saveable) || !(page.ViewModel is IHasChanges hasChanges) || !hasChanges.HasChanges)
			return true;

		string toSave = "Сохранить";
		string notToSave = "Не сохранять";
		var answer = interactiveQuestion.Question(new[] { toSave, notToSave },
			$"На вкладке есть изменения. {toSave} изменения перед закрытием?", page.ViewModel.Title);
		if(answer == toSave)
			return saveable.Save();
		return answer == notToSave;
	}

	// View сама переключает CurrentPage, этот метод для внутреннего переключения
	public override void SwitchOn(IPage page) {
		if(page is IAvaloniaWindowPage windowPage) {
			windowPage.Window?.Activate();
			return;
		}
		if(!Pages.Contains((IAvaloniaPage)page))
			OpenPage(null, page);
		else
			CurrentPage = page;
	}

	protected override IViewModelsPageFactory GetPageFactory<TViewModel>() {
		if(forceWindow || typeof(TViewModel).IsAssignableTo<IWindowDialogSettings>())
			return windowFactory;
		else
			return tabFactory;
	}

	protected override void OpenPage(IPage masterPage, IPage page) {
		pages.Add(page);

		if(page is IAvaloniaWindowPage windowPage) {
			OpenWindowPage(masterPage, windowPage);
			return;
		}

		Pages.Add(ResolveView(page));
		CurrentPage = page;
	}

	protected override void OpenSlavePage(IPage masterPage, IPage page) {
		pages.Add(page);

		if(page is IAvaloniaWindowPage windowPage) {
			OpenWindowPage(masterPage, windowPage);
			return;
		}

		var avaloniaPage = ResolveView(page);
		int masterIndex = masterPage is IAvaloniaPage masterTab ? Pages.IndexOf(masterTab) : -1;
		if(masterIndex >= 0)
			Pages.Insert(masterIndex + 1, avaloniaPage);
		else
			Pages.Add(avaloniaPage);
		CurrentPage = page;
	}

	IAvaloniaPage ResolveView(IPage page) {
		var avaloniaPage = (IAvaloniaPage)page;
		avaloniaPage.View = viewResolver.Resolve(page.ViewModel);
		if(avaloniaPage.View == null)
			throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");
		return avaloniaPage;
	}

	#region WindowDialogs

	bool forceWindow;
	Action<Window>? configureWindow;

	public IPage<TViewModel> OpenViewModelAsWindow<TViewModel>(
		IDialogViewModel master,
		OpenPageOptions options = OpenPageOptions.None,
		Action<TViewModel>? configureViewModel = null,
		Action<Window>? configureWindow = null) where TViewModel : IDialogViewModel {
		forceWindow = true;
		this.configureWindow = configureWindow;
		try {
			return OpenViewModel<TViewModel>(master, options, configureViewModel);
		}
		finally {
			forceWindow = false;
			this.configureWindow = null;
		}
	}

	public IPage<TViewModel> OpenViewModelAsWindow<TViewModel, TCtorArg1>(
		IDialogViewModel master,
		TCtorArg1 arg1,
		OpenPageOptions options = OpenPageOptions.None,
		Action<TViewModel>? configureViewModel = null,
		Action<Window>? configureWindow = null) where TViewModel : IDialogViewModel {
		forceWindow = true;
		this.configureWindow = configureWindow;
		try {
			return OpenViewModel<TViewModel, TCtorArg1>(master, arg1, options, configureViewModel);
		}
		finally {
			forceWindow = false;
			this.configureWindow = null;
		}
	}

	void OpenWindowPage(IPage? masterPage, IAvaloniaWindowPage page) {
		page.View = viewResolver.Resolve(page.ViewModel);
		if(page.View == null)
			throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");

		var window = new AvaloniaPageWindow(page, CanClosePage, closing => ClosePage(closing, CloseSource.ClosePage));
		page.Window = window;
		configureWindow?.Invoke(window);

		// VM без IWindowDialogSettings (открытые через OpenViewModelAsWindow) считаем модальными
		ShowWindow(masterPage, window, (page.ViewModel as IWindowDialogSettings)?.IsModal ?? true);
	}

	// Если диалог открыт из другого оконного диалога — владелец он, а не главное окно:
	// так модальность блокирует именно то окно, из которого открыли.
	static void ShowWindow(IPage? masterPage, Window window, bool isModal) {
		var owner = (masterPage as IAvaloniaWindowPage)?.Window
			?? (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if(owner == null)
			window.Show();
		else if(isModal)
			_ = window.ShowDialog(owner);
		else
			window.Show(owner);
	}

	protected override void ClosePage(IPage page, CloseSource source) {
		foreach(var pair in page.SlavePagesAll.ToList())
			AskClosePage(pair.SlavePage, CloseSource.WithMasterPage);

		base.ClosePage(page, source);

		if(!(page is IAvaloniaWindowPage windowPage) || windowPage.Window == null)
			return;

		var window = windowPage.Window;
		windowPage.Window = null;
		window.Close();
	}

	#endregion
}
