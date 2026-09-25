using Autofac;
using Avalonia.Controls;
using Avalonia.Threading;
using QS.Dialog;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace QS.Navigation;

public class AvaloniaNavigationManager : NavigationManagerBase, INavigationManager {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	IPage? currentPage;
	public IPage? CurrentPage
	{
		get => currentPage;
		set {
			if(ReferenceEquals(currentPage, value))
				return;

			var timer = Stopwatch.StartNew();
			var previousTitle = PageTitle(currentPage);
			var currentTitle = PageTitle(value);
			this.RaiseAndSetIfChanged(ref currentPage, value);
			Dispatcher.UIThread.Post(() =>
				logger.Debug($"Avalonia navigation: переключение «{previousTitle}» -> «{currentTitle}» " +
					$"достигло очереди Render за {timer.Elapsed.TotalMilliseconds:F0} мс."), DispatcherPriority.Render);
		}
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
		: base(interactive, hashGenerator)
	{
		this.tabFactory = tabFactory;
		this.windowFactory = windowFactory;
		this.viewResolver = viewResolver ?? throw new ArgumentNullException(nameof(viewResolver));
		this.interactiveQuestion = interactiveQuestion;
	}

	public bool AskClosePage(IPage page, CloseSource source = CloseSource.External) {
		if(!CanClosePage(page, source))
			return false;
		if(source != CloseSource.WithMasterPage && !ConfirmUnsavedChanges(page))
			return false;
		ClosePageNow(page, source);
		return true;
	}

	public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
		if(CanClosePage(page, source))
			ClosePageNow(page, source);
	}

	void ClosePageNow(IPage page, CloseSource source) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Invoke(() => ClosePageNow(page, source));
			return;
		}

		var timer = Stopwatch.StartNew();
		var pageTitle = PageTitle(page);
		if(page is IAvaloniaWindowPage) {
			ClosePage(page, source);
			return;
		}
		var avaloniaPage = (IAvaloniaPage)page;

		if(CurrentPage == page)
			CurrentPage = NextCurrentPage(avaloniaPage);
		Pages.Remove(avaloniaPage);
		ClosePage(page, source);
		Dispatcher.UIThread.Post(() =>
			logger.Debug($"Avalonia navigation: закрытие вкладки «{pageTitle}» достигло очереди Render за " +
				$"{timer.Elapsed.TotalMilliseconds:F0} мс."), DispatcherPriority.Render);
	}

	// после закрытия подчинённой возвращаемся на хозяйскую, иначе на соседнюю вкладку
	IPage? NextCurrentPage(IAvaloniaPage closing) {
		var master = SlavePages.FirstOrDefault(x => x.SlavePage == closing)?.MasterPage;
		if(master is IAvaloniaPage masterTab && Pages.Contains(masterTab))
			return master;

		// на место закрытой встаёт соседняя справа, а у последней вкладки — соседняя слева
		var rest = Pages.Where(x => x != closing).ToList();
		return rest.ElementAtOrDefault(Math.Min(Pages.IndexOf(closing), rest.Count - 1));
	}

	bool ConfirmUnsavedChanges(IPage page) {
		var askSave = (page.ViewModel as IAskSaveOnCloseViewModel)?.AskSaveOnClose ?? true;
		if(interactiveQuestion == null || !askSave)
			return true;
		if(!(page.ViewModel is UowDialogViewModelBase dialog) || !dialog.HasChanges)
			return true;

		string toSave = "Сохранить";
		string notToSave = "Не сохранять";
		var answer = interactiveQuestion.Question(new[] { toSave, notToSave },
			$"На вкладке есть изменения. {toSave} изменения перед закрытием?", page.ViewModel.Title);
		if(answer == toSave)
			return dialog.Save();
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
		if(typeof(TViewModel).IsAssignableTo<IWindowDialogSettings>())
			return windowFactory;
		else
			return tabFactory;
	}

	protected override void OpenPage(IPage masterPage, IPage page) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Invoke(() => OpenPage(masterPage, page));
			return;
		}
		pages.Add(page);

		if(page is IAvaloniaWindowPage windowPage) {
			OpenWindowPage(masterPage, windowPage);
			return;
		}

		Pages.Add(ResolveView(page));
		CurrentPage = page;
	}

	protected override void OpenSlavePage(IPage masterPage, IPage page) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Invoke(() => OpenSlavePage(masterPage, page));
			return;
		}
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
		avaloniaPage.View = MakePageContent(page);
		return avaloniaPage;
	}

	Control MakePageContent(IPage page) {
		var view = viewResolver.Resolve(page.ViewModel);
		if(view == null)
			throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");

		return PageView.Wrap(view);
	}

	#region WindowDialogs

	public IPage<TViewModel> OpenViewModelAsWindow<TViewModel>(
		IDialogViewModel master,
		OpenPageOptions options = OpenPageOptions.None,
		Action<TViewModel>? configureViewModel = null,
		Action<Window>? configureWindow = null) where TViewModel : IDialogViewModel =>
		OpenAsWindow(master, Type.EmptyTypes, Array.Empty<object>(), options, configureViewModel, configureWindow);

	public IPage<TViewModel> OpenViewModelAsWindow<TViewModel, TCtorArg1>(
		IDialogViewModel master,
		TCtorArg1 arg1,
		OpenPageOptions options = OpenPageOptions.None,
		Action<TViewModel>? configureViewModel = null,
		Action<Window>? configureWindow = null) where TViewModel : IDialogViewModel =>
		OpenAsWindow(master, new[] { typeof(TCtorArg1) }, new object?[] { arg1 }, options, configureViewModel, configureWindow);

	IPage<TViewModel> OpenAsWindow<TViewModel>(IDialogViewModel master, Type[] ctorTypes, object?[] ctorValues,
		OpenPageOptions options, Action<TViewModel>? configureViewModel, Action<Window>? configureWindow)
		where TViewModel : IDialogViewModel =>
		(IPage<TViewModel>)OpenViewModelInternal(FindPage(master), options,
			() => hashGenerator?.GetHash<TViewModel>(master, ctorTypes, ctorValues),
			hash => {
				var page = windowFactory.CreateViewModelTypedArgs(master, ctorTypes, ctorValues, hash, null, configureViewModel);
				((IAvaloniaWindowPage)page).ConfigureWindow = configureWindow;
				return page;
			});

	void OpenWindowPage(IPage? masterPage, IAvaloniaWindowPage page) {
		page.View = MakePageContent(page);

		var window = new AvaloniaPageWindow(page,
			closing => CanClosePage(closing, CloseSource.ClosePage) && ConfirmUnsavedChanges(closing),
			closing => ClosePage(closing, CloseSource.ClosePage));
		page.Window = window;
		page.ConfigureWindow?.Invoke(window);
		window.Open(masterPage);
	}

	protected override void ClosePage(IPage page, CloseSource source) {
		// подчинённые уходят вместе с хозяйской без проверок, как ForceCloseTab в TdiNotebook
		foreach(var pair in page.SlavePagesAll.ToList())
			ClosePageNow(pair.SlavePage, CloseSource.WithMasterPage);

		base.ClosePage(page, source);
		PageView.DisposeOnClose(page);

		if(!(page is IAvaloniaWindowPage windowPage) || windowPage.Window == null)
			return;

		var window = windowPage.Window;
		windowPage.Window = null;
		window.Close();
	}

	private static string PageTitle(IPage? page) => page?.Title ?? "нет";

	#endregion
}
