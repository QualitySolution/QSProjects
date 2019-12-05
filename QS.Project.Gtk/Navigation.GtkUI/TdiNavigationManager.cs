using System;
using System.Collections.Generic;
using System.Linq;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;

namespace QS.Navigation.GtkUI
{
	public class TdiNavigationManager : INavigationManager, ITdiCompatibilityNavigation
	{
		readonly TdiNotebook tdiNotebook;
		readonly IPageHashGenerator hashGenerator;
		readonly IViewModelsPageFactory viewModelsFactory;

		public TdiNavigationManager(TdiNotebook tdiNotebook, IPageHashGenerator hashGenerator, IViewModelsPageFactory viewModelsFactory)
		{
			this.tdiNotebook = tdiNotebook;
			this.hashGenerator = hashGenerator;
			this.viewModelsFactory = viewModelsFactory;

			tdiNotebook.TabClosed += TdiNotebook_TabClosed;
		}

		#region Перебор страниц
		private readonly List<IPage> pages = new List<IPage>();

		public IEnumerable<IPage> AllPages {
			get {
				foreach (var page in pages) {
					yield return page;
					foreach (var child in page.ChildPagesAll)
						yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<IPage> TopLevelPages => pages;

		public IEnumerable<IPage> IndependentPages {
			get {
				foreach (var page in pages) {
					if (SlavePages.All(x => x.SlavePage != page))
						yield return page;
					foreach (var child in page.ChildPagesAll)
						if (SlavePages.All(x => x.SlavePage != child))
							yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<MasterToSlavePair> SlavePages {
			get {
				foreach (var page in pages) {
					foreach (var slavePair in page.SlavePagesAll)
						yield return slavePair;
				}
			}
		}

		public IEnumerable<ParentToChildPair> ChildPages {
			get {
				foreach (var page in pages) {
					foreach (var childPair in page.ChildPagesAll)
						yield return childPair;
				}
			}
		}

		#endregion

		#region Закрытие

		public bool AskClosePage(IPage page)
		{
			return tdiNotebook.AskToCloseTab((ITdiTab)page.ViewModel);
		}

		public void ForceClosePage(IPage page)
		{
			tdiNotebook.ForceCloseTab((ITdiTab)page.ViewModel);
		}

		void TdiNotebook_TabClosed(object sender, TabClosedEventArgs e)
		{
			ITdiTab closedTab;
			if(e.Tab is TdiSliderTab tdiSlider)
				closedTab = tdiSlider.Journal;
			else
				closedTab = e.Tab;

			var closedPagePair = SlavePages.FirstOrDefault(x => x.SlavePage.ViewModel == closedTab);
			if (closedPagePair != null)
				(closedPagePair.MasterPage as IPageInternal).RemoveSlavePage(closedPagePair.SlavePage);
			var pageToRemove = pages.FirstOrDefault(x => x.ViewModel == closedTab);
			if(pageToRemove != null) {
				pages.Remove(pageToRemove);
				(pageToRemove as IPageInternal).OnClosed();
			}
			else {
				var childPair = ChildPages.FirstOrDefault(x => x.ChildPage.ViewModel == closedTab);
				if(childPair != null) {
					(childPair.ParentPage as IPageInternal).RemoveChildPage(childPair.ChildPage);
					(childPair.ChildPage as IPageInternal).OnClosed();
				}
			}
		}

		#endregion

		#region Поиск

		public IPage FindPage(ViewModelBase viewModel)
		{
			return AllPages.FirstOrDefault(x => x.ViewModel == viewModel);
		}

		public IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			return FindPage(viewModel);
		}

		#endregion

		#region Открытие

		public IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, TCtorArg1 arg3, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2), typeof(TCtorArg3) };
			var values = new object[] { arg1, arg2, arg3 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				FindPage(master), options, 
				() => hashGenerator.GetHash<TViewModel>(master, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, hash)
			);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				FindPage(master), options,
				() => hashGenerator.GetHashNamedArgs<TViewModel>(master, ctorArgs),
				(hash) => viewModelsFactory.CreateViewModelNamedArgs<TViewModel>(master, ctorArgs, hash)
			);
		}

		#region Внутренне

		private IPage<TViewModel> OpenViewModelInternal<TViewModel>(IPage masterPage, OpenViewModelOptions options, Func<string> makeHash, Func<string, IPage<TViewModel>> makeViewModelPage) where TViewModel : ViewModelBase
		{
			string hash = null;
			if (!options.HasFlag(OpenViewModelOptions.IgnoreHash))
				hash = makeHash();

			IPage openPage = null;

			if (options.HasFlag(OpenViewModelOptions.AsSlave)) {
				if (masterPage == null)
					throw new InvalidOperationException($"Отсутствует главная страница для добавляемой подчиненой страницы.");

				if (hash != null)
					openPage = masterPage.SlavePagesAll.Select(x => x.SlavePage).FirstOrDefault(x => x.PageHash == hash);
				if (openPage != null)
					SwitchOn(openPage);
				else {
					openPage = makeViewModelPage(hash);
					tdiNotebook.AddSlaveTab((masterPage as ITdiPage).TdiTab, (ITdiTab)openPage.ViewModel);
					(masterPage as IPageInternal).AddSlavePage(openPage);
					pages.Add(openPage);
				}
			}
			else {
				if (hash != null)
					openPage = IndependentPages.FirstOrDefault(x => x.PageHash == hash);
				if (openPage != null)
					SwitchOn(openPage);
				else {
					openPage = makeViewModelPage(hash);
					var masterTab = (masterPage as ITdiPage)?.TdiTab;
					if (masterTab is ITdiJournal && masterTab.TabParent is TdiSliderTab) {
						var slider = masterTab.TabParent as TdiSliderTab;
						slider.AddTab((ITdiTab)openPage.ViewModel, masterTab);
						(masterPage as IPageInternal).AddChildPage(openPage);
					}
					else {
						tdiNotebook.AddTab((ITdiTab)openPage.ViewModel, masterTab);
						pages.Add(openPage);
					}
				}
			}
			return (IPage<TViewModel>)openPage;
		}
		#endregion

		#region ITdiCompatibilityNavigation

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(ITdiTab master, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				FindOrCreateMasterPage(master), options,
				() => hashGenerator.GetHash<TViewModel>(null, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(null, ctorTypes, ctorValues, hash)
			);
		}

		private IPage FindOrCreateMasterPage(ITdiTab tab)
		{
			ITdiPage page = AllPages.OfType<ITdiPage>().FirstOrDefault(x => x.TdiTab == tab);
			if(page == null)
				page = new TdiTabPage(tab);

			return (IPage)page;
		}

		#endregion

		#endregion

		public void SwitchOn(IPage page)
		{
			tdiNotebook.SwitchOnTab((ITdiTab)page.ViewModel);
		}
	}
}