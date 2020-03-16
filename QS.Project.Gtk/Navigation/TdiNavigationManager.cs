using System;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels.Dialog;
using QS.Views.Dialog;
using QS.Views.Resolve;

namespace QS.Navigation
{
	public class TdiNavigationManager : NavigationManagerBase, INavigationManager, ITdiCompatibilityNavigation
	{
		readonly TdiNotebook tdiNotebook;
		readonly IGtkViewResolver viewResolver;

		protected readonly IViewModelsPageFactory viewModelsFactory;
		protected readonly AutofacViewModelsGtkPageFactory viewModelsGtkWindowsFactory;
		//Только для режима смешанного использования Tdi и ViewModel 
		readonly ITdiPageFactory tdiPageFactory;

		public TdiNavigationManager(
			TdiNotebook tdiNotebook,
			IViewModelsPageFactory viewModelsFactory,
			IInteractiveMessage interactive,
			IPageHashGenerator hashGenerator = null,
			ITdiPageFactory tdiPageFactory = null,
			AutofacViewModelsGtkPageFactory viewModelsGtkPageFactory = null, 
			IGtkViewResolver viewResolver = null)
			: base(interactive, hashGenerator)
		{
			this.tdiNotebook = tdiNotebook ?? throw new ArgumentNullException(nameof(tdiNotebook));
			this.tdiPageFactory = tdiPageFactory;
			this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
			this.viewModelsGtkWindowsFactory = viewModelsGtkPageFactory;
			this.viewResolver = viewResolver;

			tdiNotebook.TabClosed += TdiNotebook_TabClosed;
		}

		#region Закрытие

		public bool AskClosePage(IPage page, CloseSource source = CloseSource.External)
		{
			if (page is ITdiPage tdiPage)
				return tdiNotebook.AskToCloseTab(tdiPage.TdiTab, source);
			else
				ClosePage(page, source);
			return true;
		}

		public void ForceClosePage(IPage page, CloseSource source = CloseSource.External)
		{
			if (page is ITdiPage tdiPage)
				tdiNotebook.ForceCloseTab(tdiPage.TdiTab, source);
			else
				ClosePage(page, source);
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
				ClosePage(page, e.CloseSource);
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

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1>(ITdiTab master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1, TCtorArg2>(ITdiTab master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenViewModelOnTdiTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindOrCreateMasterPage(master), options,
				() => hashGenerator?.GetHash<TViewModel>(null, ctorTypes, ctorValues),
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
				() => hashGenerator?.GetHash<TTab>(null, ctorTypes, ctorValues),
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
			if(page == null) {
				page = new TdiTabPage(tab, null);
				pages.Add(page);
			}

			return (IPage)page;
		}

		#endregion

		public override void SwitchOn(IPage page)
		{
			tdiNotebook.SwitchOnTab((page as ITdiPage).TdiTab);
		}

		protected override void OpenSlavePage(IPage masterPage, IPage page)
		{
			pages.Add(page);
			if(page.ViewModel is ModalDialogViewModelBase)
				OpenModalPage(page);
			else
				tdiNotebook.AddSlaveTab((masterPage as ITdiPage).TdiTab, (page as ITdiPage).TdiTab);
		}

		protected override void OpenPage(IPage masterPage, IPage page)
		{
			if(page.ViewModel is ModalDialogViewModelBase) {
				pages.Add(page);
				OpenModalPage(page);
				return;
			}

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

		protected override IViewModelsPageFactory GetPageFactory<TViewModel>()
		{
			if(typeof(TViewModel).IsAssignableTo<ModalDialogViewModelBase>())
				return viewModelsGtkWindowsFactory;
			else
				return viewModelsFactory;

		}

		#region ModalDialogs

		protected void OpenModalPage(IPage page)
		{
			var gtkPage = (IGtkWindowPage)page;
			gtkPage.GtkView = viewResolver.Resolve(page.ViewModel);
			if(gtkPage.GtkView == null)
				throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");
			gtkPage.GtkDialog = new Gtk.Dialog(gtkPage.ViewModel.Title, tdiNotebook.Toplevel as Window, DialogFlags.Modal);
			var defaultsize = gtkPage.GtkView.GetType().GetAttribute<WindowSizeAttribute>(true);
			gtkPage.GtkDialog.SetDefaultSize(defaultsize?.DefaultWidth ?? gtkPage.GtkView.WidthRequest, defaultsize?.DefaultHeight ?? gtkPage.GtkView.WidthRequest);
			gtkPage.GtkDialog.VBox.Add(gtkPage.GtkView);
			gtkPage.GtkView.Show();
			gtkPage.GtkDialog.Show();
			gtkPage.GtkDialog.DeleteEvent += GtkDialog_DeleteEvent;
			gtkPage.ViewModel.PropertyChanged += (sender, e) => gtkPage.GtkDialog.Title = gtkPage.ViewModel.Title;
		}

		void GtkDialog_DeleteEvent(object o, DeleteEventArgs args)
		{
			var page = FindPage(args.Event.Window) ?? throw new InvalidOperationException("Закрыто окно которое не зарегистрировано как страницы в навигаторе");
			ClosePage(page, CloseSource.ClosePage);
		}

		public IPage FindPage(Gdk.Window window)
		{
			return AllPages.OfType<IGtkWindowPage>().FirstOrDefault(x => x.GtkDialog.GdkWindow == window);
		}

		protected override void ClosePage(IPage page, CloseSource source)
		{
			base.ClosePage(page, source);

			if(page is IGtkWindowPage gtkPage) {
				gtkPage.GtkDialog.Respond((int)ResponseType.DeleteEvent);
				gtkPage.GtkView.Destroy();
				gtkPage.GtkDialog.Destroy();
			}
		}

		#endregion
	}
}