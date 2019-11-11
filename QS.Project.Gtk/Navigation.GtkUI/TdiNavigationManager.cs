using System;
using System.Collections.Generic;
using System.Linq;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;

namespace QS.Navigation.GtkUI
{
	public class TdiNavigationManager : INavigationManager
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
					foreach (var child in page.ChildPages)
						yield return child;
				}
			}
		}

		public IEnumerable<IPage> TopLevelPages => pages;

		public IEnumerable<IPage> IndependentPages {
			get {
				foreach (var page in pages) {
					if (SlavePages.All(x => x.SlavePage != page))
						yield return page;
					foreach (var child in page.ChildPages)
						if (SlavePages.All(x => x.SlavePage != child))
							yield return child;
				}
			}
		}

		public IEnumerable<MasterToSlavePair> SlavePages {
			get {
				foreach (var page in pages) {
					foreach (var slave in page.SlavePages)
						yield return new MasterToSlavePair { MasterPage = page, SlavePage = slave };
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
			var masterOfClosedPage = SlavePages.FirstOrDefault(x => x.SlavePage.ViewModel == e.Tab);
			if (masterOfClosedPage != null)
				masterOfClosedPage.MasterPage.SlavePages.Remove(masterOfClosedPage.SlavePage);
			var pageToRemove = pages.FirstOrDefault(x => x.ViewModel == e.Tab);
			if(pageToRemove != null) {
				pages.Remove(pageToRemove);
				(pageToRemove as IPageInternal).OnClosed();
			}
		}

		#endregion

		#region Поиск

		public IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			foreach (var page in pages) {
				var result = FingOnPage<TViewModel>(page, viewModel);
				if (result != null)
					return result;
			}
			return null;
		}

		private IPage<TViewModel> FingOnPage<TViewModel>(IPage page, ViewModelBase viewModel)
			where TViewModel : ViewModelBase
		{
			if (page.ViewModel == viewModel)
				return (IPage<TViewModel>)page;

			foreach (var child in page.ChildPages) {
				var result = FingOnPage<TViewModel>(child, viewModel);
				if (result != null)
					return result;
			}
			return null;
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
				master, options, 
				() => hashGenerator.GetHash<TViewModel>(master, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, hash)
			);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase
		{
			return OpenViewModelInternal<TViewModel>(
				master, options,
				() => hashGenerator.GetHashNamedArgs<TViewModel>(master, ctorArgs),
				(hash) => viewModelsFactory.CreateViewModelNamedArgs<TViewModel>(master, ctorArgs, hash)
			);
		}

		#region Внутренне

		private IPage<TViewModel> OpenViewModelInternal<TViewModel>(ViewModelBase master, OpenViewModelOptions options, Func<string> makeHash, Func<string, IPage<TViewModel>> makeViewModelPage) where TViewModel : ViewModelBase
		{
			string hash = null;
			if (!options.HasFlag(OpenViewModelOptions.IgnoreHash))
				hash = makeHash();

			IPage openPage = null;

			if (options.HasFlag(OpenViewModelOptions.AsSlave)) {
				var masterPage = FindPage(master);
				if (masterPage == null)
					throw new InvalidOperationException($"Страница для {master} не найдена в менеджере.");

				if (hash != null)
					openPage = masterPage.SlavePages.First(x => x.PageHash == hash);
				if (openPage != null)
					SwitchOn(openPage);
				else {
					openPage = makeViewModelPage(hash);
					tdiNotebook.AddSlaveTab((ITdiTab)master, (ITdiTab)openPage.ViewModel);
					masterPage.SlavePages.Add(openPage);
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
					if(master is ITdiJournal && (master as ITdiTab).TabParent is TdiSliderTab) {
						var slider = (master as ITdiTab).TabParent as TdiSliderTab;
						slider.AddTab((ITdiTab)openPage.ViewModel, (ITdiTab)master);
					}
					else {
						tdiNotebook.AddTab((ITdiTab)openPage.ViewModel, (ITdiTab)master);
						pages.Add(openPage);
					}
				}
			}
			return (IPage<TViewModel>)openPage;
		}
		#endregion

		#endregion

		public void SwitchOn(IPage page)
		{
			tdiNotebook.SwitchOnTab((ITdiTab)page.ViewModel);
		}
	}

	public class MasterToSlavePair
	{
		public IPage MasterPage;
		public IPage SlavePage;
	}
}