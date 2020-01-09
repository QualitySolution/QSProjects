using System;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;

namespace QS.Navigation
{
	public class TdiNavigationManager : NavigationManagerBase, INavigationManager, ITdiCompatibilityNavigation
	{
		readonly TdiNotebook tdiNotebook;

		//Только для режима смешанного использования Tdi и ViewModel 
		readonly ITdiPageFactory tdiPageFactory;

		public TdiNavigationManager(TdiNotebook tdiNotebook, IPageHashGenerator hashGenerator, IViewModelsPageFactory viewModelsFactory, IInteractiveMessage interactive, ITdiPageFactory tdiPageFactory = null)
			: base(hashGenerator, viewModelsFactory, interactive)
		{
			this.tdiNotebook = tdiNotebook ?? throw new ArgumentNullException(nameof(tdiNotebook));
			this.tdiPageFactory = tdiPageFactory;

			tdiNotebook.TabClosed += TdiNotebook_TabClosed;
		}

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

			var page = FindPage(closedTab);
			if (page != null)
				ClosePage(page);
		}

		#endregion

		#region ITdiCompatibilityNavigation

		#region Открытие ViewModel

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(ITdiTab master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindOrCreateMasterPage(master), options,
				() => hashGenerator.GetHash<TViewModel>(null, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(null, ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		#endregion

		#region Открытие TdiTab

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenTdiTab<TTab>(masterTab, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenTdiTab<TTab>(masterTab, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindOrCreateMasterPage(masterTab), options,
				() => hashGenerator.GetHash<TTab>(null, ctorTypes, ctorValues),
				(hash) => tdiPageFactory.CreateTdiPageTypedArgs<TTab>(ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		#endregion

		private IPage FindPage(ITdiTab tab)
		{
			return AllPages.OfType<ITdiPage>().FirstOrDefault(x => x.TdiTab == tab);
		}

		private IPage FindOrCreateMasterPage(ITdiTab tab)
		{
			ITdiPage page = AllPages.OfType<ITdiPage>().FirstOrDefault(x => x.TdiTab == tab);
			if(page == null)
				page = new TdiTabPage(tab, null);

			return (IPage)page;
		}

		#endregion

		public override void SwitchOn(IPage page)
		{
			tdiNotebook.SwitchOnTab((ITdiTab)page.ViewModel);
		}

		protected override void OpenSlavePage(IPage masterPage, IPage page)
		{
			pages.Add(page);
			tdiNotebook.AddSlaveTab((masterPage as ITdiPage).TdiTab, (page as ITdiPage).TdiTab);
		}

		protected override void OpenPage(IPage masterPage, IPage page)
		{
			var masterTab = (masterPage as ITdiPage)?.TdiTab;

			if (masterTab is ITdiJournal && masterTab.TabParent is TdiSliderTab) {
				var slider = masterTab.TabParent as TdiSliderTab;
				slider.AddTab((page as ITdiPage).TdiTab, masterTab);
				(masterPage as IPageInternal).AddChildPage(page);
			}
			else {
				pages.Add(page);
				tdiNotebook.AddTab((page as ITdiPage).TdiTab, (masterPage as ITdiPage)?.TdiTab);
			}
		}
	}
}