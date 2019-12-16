using System;
using System.Collections.Generic;
using System.Linq;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;

namespace QS.Navigation
{
	public class TdiNavigationManager : INavigationManager, ITdiCompatibilityNavigation
	{
		readonly TdiNotebook tdiNotebook;
		readonly IPageHashGenerator hashGenerator;
		readonly IViewModelsPageFactory viewModelsFactory;
		//Только для режима смешанного использования Tdi и ViewModel 
		readonly ITdiPageFactory tdiPageFactory;

		public TdiNavigationManager(TdiNotebook tdiNotebook, IPageHashGenerator hashGenerator, IViewModelsPageFactory viewModelsFactory, ITdiPageFactory tdiPageFactory = null)
		{
			this.tdiNotebook = tdiNotebook ?? throw new ArgumentNullException(nameof(tdiNotebook));
			this.hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
			this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
			this.tdiPageFactory = tdiPageFactory;

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

			var closedPagePair = SlavePages.FirstOrDefault(x => (x.SlavePage as ITdiPage).TdiTab == closedTab);
			if (closedPagePair != null)
				(closedPagePair.MasterPage as IPageInternal).RemoveSlavePage(closedPagePair.SlavePage);
			var pageToRemove = pages.Cast<ITdiPage>().FirstOrDefault(x => x.TdiTab == closedTab);
			if(pageToRemove != null) {
				pages.Remove(pageToRemove);
				(pageToRemove as IPageInternal).OnClosed();
			}
			else {
				var childPair = ChildPages.FirstOrDefault(x => (x.ChildPage as ITdiPage).TdiTab == closedTab);
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

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, TCtorArg1 arg3, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2), typeof(TCtorArg3) };
			var values = new object[] { arg1, arg2, arg3 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindPage(master), options, 
				() => hashGenerator.GetHash<TViewModel>(master, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, hash)
			);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator.GetHashNamedArgs<TViewModel>(master, ctorArgs),
				(hash) => viewModelsFactory.CreateViewModelNamedArgs<TViewModel>(master, ctorArgs, hash)
			);
		}

		#region Внутренне

		private IPage OpenViewModelInternal(IPage masterPage, OpenPageOptions options, Func<string> makeHash, Func<string, IPage> makePage)
		{
			string hash = null;
			if (!options.HasFlag(OpenPageOptions.IgnoreHash))
				hash = makeHash();

			ITdiPage openPage = null;

			if (options.HasFlag(OpenPageOptions.AsSlave)) {
				if (masterPage == null)
					throw new InvalidOperationException($"Отсутствует главная страница для добавляемой подчиненой страницы.");

				if (hash != null)
					openPage = (ITdiPage)masterPage.SlavePagesAll.Select(x => x.SlavePage).FirstOrDefault(x => x.PageHash == hash);
				if (openPage != null)
					SwitchOn(openPage);
				else {
					openPage = (ITdiPage)makePage(hash);
					tdiNotebook.AddSlaveTab((masterPage as ITdiPage).TdiTab, openPage.TdiTab);
					(masterPage as IPageInternal).AddSlavePage(openPage);
					pages.Add(openPage);
				}
			}
			else {
				if (hash != null)
					openPage = (ITdiPage)IndependentPages.FirstOrDefault(x => x.PageHash == hash);
				if (openPage != null)
					SwitchOn(openPage);
				else {
					openPage = (ITdiPage)makePage(hash);
					var masterTab = (masterPage as ITdiPage)?.TdiTab;
					if (masterTab is ITdiJournal && masterTab.TabParent is TdiSliderTab) {
						var slider = masterTab.TabParent as TdiSliderTab;
						slider.AddTab(openPage.TdiTab, masterTab);
						(masterPage as IPageInternal).AddChildPage(openPage);
					}
					else {
						tdiNotebook.AddTab(openPage.TdiTab, masterTab);
						pages.Add(openPage);
					}
				}
			}
			return openPage;
		}
		#endregion

		#region ITdiCompatibilityNavigation

		#region Открытие ViewModel

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(ITdiTab master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, types, values, options);
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindOrCreateMasterPage(master), options,
				() => hashGenerator.GetHash<TViewModel>(null, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(null, ctorTypes, ctorValues, hash)
			);
		}

		#endregion

		#region Открытие TdiTab

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None)
			where TTab : ITdiTab
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenTdiTab<TTab>(masterTab, types, values, options);
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None)
			where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenTdiTab<TTab>(masterTab, types, values, options);
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None)
			where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindOrCreateMasterPage(masterTab), options,
				() => hashGenerator.GetHash<TTab>(null, ctorTypes, ctorValues),
				(hash) => tdiPageFactory.CreateTdiPageTypedArgs<TTab>(ctorTypes, ctorValues, hash)
			);
		}

		#endregion

		private IPage FindOrCreateMasterPage(ITdiTab tab)
		{
			ITdiPage page = AllPages.OfType<ITdiPage>().FirstOrDefault(x => x.TdiTab == tab);
			if(page == null)
				page = new TdiTabPage(tab, null);

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