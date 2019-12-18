using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels;
using QS.Services;
using QS.Project.Journal;
using QS.DomainModel.Entity;
using QS.Dialog;

namespace QS.Navigation.TabNavigation
{
	public class TabNavigator : PropertyChangedBase, INavigationManager
	{
		private readonly IPageHashGenerator hashGenerator;
		private readonly IViewModelsPageFactory viewModelsFactory;
		private readonly IInteractiveService interactiveService;

		public TabNavigator(IPageHashGenerator hashGenerator, IViewModelsPageFactory viewModelsFactory, IInteractiveService interactiveService)
		{
			this.hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
			this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		private IPage activePage;
		public virtual IPage ActivePage {
			get => activePage;
			set => SetField(ref activePage, value);
		}


		#region Перебор страниц

		private readonly List<IPage> pages = new List<IPage>();

		public event EventHandler PageChanged;

		public IEnumerable<IPage> AllPages {
			get {
				foreach(var page in pages) {
					yield return page;
					foreach(var child in page.ChildPagesAll)
						yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<IPage> TopLevelPages => pages;

		public IEnumerable<IPage> IndependentPages {
			get {
				foreach(var page in pages) {
					if(SlavePages.All(x => x.SlavePage != page))
						yield return page;
					foreach(var child in page.ChildPagesAll)
						if(SlavePages.All(x => x.SlavePage != child))
							yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<MasterToSlavePair> SlavePages {
			get {
				foreach(var page in pages) {
					foreach(var slavePair in page.SlavePagesAll)
						yield return slavePair;
				}
			}
		}

		public IEnumerable<ParentToChildPair> ChildPages {
			get {
				foreach(var page in pages) {
					foreach(var childPair in page.ChildPagesAll)
						yield return childPair;
				}
			}
		}

		#endregion

		#region INavigationManager implementation

		public IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			return AllPages.FirstOrDefault(x => x.ViewModel == viewModel) as IPage<TViewModel>;
		}

		public void SwitchOn(IPage page)
		{
			if(AllPages.Contains(page)) {
				ActivePage = page;
			}
		}

		#region Open

		public IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2), typeof(TCtorArg3) };
			var values = new object[] { arg1, arg2, arg3 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				FindPage(master), options,
				() => hashGenerator.GetHash<TViewModel>(master, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, hash)
			);
		}

		public IPage OpenViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None)
		{
			return OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator.GetHash(viewModelType, master, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs(viewModelType, master, ctorTypes, ctorValues, hash)
			);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				FindPage(master), options,
				() => hashGenerator.GetHashNamedArgs<TViewModel>(master, ctorArgs),
				(hash) => viewModelsFactory.CreateViewModelNamedArgs<TViewModel>(master, ctorArgs, hash)
			);
		}

		public IPage OpenViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None)
		{
			return OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator.GetHashNamedArgs(viewModelType, master, ctorArgs),
				(hash) => viewModelsFactory.CreateViewModelNamedArgs(viewModelType, master, ctorArgs, hash)
			);
		}

		#endregion Open

		#region Close

		public bool AskClosePage(IPage page)
		{
			if(!interactiveService.InteractiveQuestion.Question("Вы действительно хотите закрыть вкладку?")) {
				return false;
			}

			if(page.SlavePages.Any()) {
				interactiveService.InteractiveMessage.ShowMessage(ImportanceLevel.Warning, "Сначала необходимо закрыть подчиненную вкладку!");
				SwitchOn(page.SlavePages.First());
				return false;
			}

			foreach(var pagePair in page.ChildPagesAll) {
				ClosePage(pagePair.ChildPage, false);
			}

			ClosePage(page, false);
			return true;
		}

		public void ForceClosePage(IPage page)
		{
			foreach(var pagePair in page.SlavePagesAll) {
				ClosePage(pagePair.SlavePage, true);
			}

			foreach(var pagePair in page.ChildPagesAll) {
				ClosePage(pagePair.ChildPage, true);
			}

			ClosePage(page, true);
		}

		protected virtual void ClosePage(IPage page, bool forceClosing)
		{
			((PageBase)page).OnClosing(forceClosing);

			bool pagesChanged = false;
			PageBase parentPage = (PageBase)FindParentPage(page);
			if(parentPage != null) {
				pagesChanged |= parentPage.RemoveChildPage(page);
			}
			PageBase masterPage = (PageBase)FindMasterPage(page);
			if(masterPage != null) {
				pagesChanged |= masterPage.RemoveSlavePage(page);
			}
			if(pagesChanged) {
				RaisePageChaged();
			}
			((PageBase)page).OnClosed();
		}

		#endregion Close

		#endregion INavigationManager implementation

		private IPage FindParentPage(IPage page)
		{
			if(page == null) {
				return null;
			}
			return ChildPages.FirstOrDefault(x => x.ChildPage.ViewModel == page.ViewModel)?.ParentPage;
		}

		private IPage FindMasterPage(IPage page)
		{
			if(page == null) {
				return null;
			}
			return SlavePages.FirstOrDefault(x => x.SlavePage.ViewModel == page.ViewModel)?.MasterPage;
		}

		private IPage<TViewModel> OpenViewModelInternal<TViewModel>(IPage masterPage, OpenPageOptions options, Func<string> makeHash, Func<string, IPage<TViewModel>> makeViewModelPage) where TViewModel : ViewModelBase
		{
			Func<string, IPage> makeViewModelPageFunc = makeViewModelPage;
			IPage page = OpenViewModelInternal(masterPage, options, makeHash, makeViewModelPageFunc);
			return (IPage<TViewModel>)page;
		}

		private IPage OpenViewModelInternal(IPage masterPage, OpenPageOptions options, Func<string> makeHash, Func<string, IPage> makeViewModelPage)
		{
			string hash = null;
			if(!options.HasFlag(OpenPageOptions.IgnoreHash))
				hash = makeHash();

			IPage openPage = null;

			if(options.HasFlag(OpenPageOptions.AsSlave)) {
				if(masterPage == null) {
					throw new InvalidOperationException($"Отсутствует главная страница для добавляемой подчиненой страницы.");
				}
				if(hash != null)
					openPage = masterPage.SlavePagesAll.Select(x => x.SlavePage).FirstOrDefault(x => x.PageHash == hash);
				if(openPage != null)
					SwitchOn(openPage);
				else {
					openPage = makeViewModelPage(hash);
					(masterPage as PageBase).AddSlavePage(openPage);
					pages.Add(openPage);
				}
			} else {
				if(hash != null)
					openPage = IndependentPages.FirstOrDefault(x => x.PageHash == hash);
				if(openPage != null)
					SwitchOn(openPage);
				else {
					openPage = makeViewModelPage(hash);

					//Открытие из журнала
					var useSliderOptions = new SliderOption[] { SliderOption.UseSlider, SliderOption.UseSliderHided };
					if(openPage.ViewModel is IJournalSlidedTab slidedViewModel && useSliderOptions.Contains(slidedViewModel.SliderOption) && masterPage != null) {
						(masterPage as PageBase).AddChildPage(openPage);
					} else {
						pages.Add(openPage);
					}
				}
			}
			RaisePageChaged();
			return openPage;
		}

		private void RaisePageChaged()
		{
			PageChanged?.Invoke(this, EventArgs.Empty);
			ActualizeActivePage();
		}

		private void ActualizeActivePage()
		{
			if(ActivePage == null) {
				return;
			}

			if(AllPages.Contains(ActivePage)) {
				return;
			}

			ActivePage = TopLevelPages.FirstOrDefault();
		}
	}
}
