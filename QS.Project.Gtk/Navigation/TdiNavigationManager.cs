using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
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
				FindOrCreatePage(master), options,
				() => hashGenerator?.GetHash<TViewModel>(null, ctorTypes, ctorValues),
				(hash) => viewModelsFactory.CreateViewModelTypedArgs<TViewModel>(null, ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		#endregion

		#region Открытие TdiTab из Tdi

		public ITdiPage OpenTdiTabOnTdi<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenTdiTabOnTdi<TTab>(masterTab, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenTdiTabOnTdi<TTab>(masterTab, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1, TCtorArg2>(ITdiTab masterTab, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenTdiTabOnTdi<TTab>(masterTab, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTabOnTdi<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null)
			where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindOrCreatePage(masterTab), options,
				() => hashGenerator?.GetHash<TTab>(null, ctorTypes, ctorValues),
				(hash) => tdiPageFactory.CreateTdiPageTypedArgs<TTab>(ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		public ITdiPage OpenTdiTabOnTdiNamedArgs<TTab>(ITdiTab masterTab, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindOrCreatePage(masterTab), options,
				() => hashGenerator?.GetHashNamedArgs<TTab>(null, ctorArgs),
				(hash) => tdiPageFactory.CreateTdiPageNamedArgs<TTab>(ctorArgs, hash, addingRegistrations)
			);
		}

		#endregion

		#region Открытие TdiTab из ViewModel

		public ITdiPage OpenTdiTab<TTab>(DialogViewModelBase master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenTdiTab<TTab>(master, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1>(DialogViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenTdiTab<TTab>(master, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTab<TTab, TCtorArg1, TCtorArg2>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenTdiTab<TTab>(master, types, values, options, addingRegistrations);
		}

		public ITdiPage OpenTdiTab<TTab>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator?.GetHash<TTab>(null, ctorTypes, ctorValues),
				(hash) => tdiPageFactory.CreateTdiPageTypedArgs<TTab>(ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		public ITdiPage OpenTdiTabNamedArgs<TTab>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab
		{
			return (ITdiPage)OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator?.GetHashNamedArgs<TTab>(null, ctorArgs),
				(hash) => tdiPageFactory.CreateTdiPageNamedArgs<TTab>(ctorArgs, hash, addingRegistrations)
			);
		}

		#endregion


		public IPage CurrentPage => FindOrCreatePage(tdiNotebook.CurrentTab);

		private IPage FindPage(ITdiTab tab)
		{
			return AllPages.OfType<ITdiPage>().FirstOrDefault(x => x.TdiTab == tab);
		}

		private IPage FindOrCreatePage(ITdiTab tab)
		{
			if (tab == null)
				return null;

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
			if(page is ITdiPage tdiPage)
				tdiNotebook.SwitchOnTab(tdiPage.TdiTab);
			else if(page is IGtkWindowPage gtkWindowPage)
				gtkWindowPage.GtkDialog.Present();
		}

		protected override void OpenSlavePage(IPage masterPage, IPage page)
		{
			if(page.ViewModel is IWindowDialogSettings) {
				pages.Add(page);
				OpenWindowPage(page);
				return;
			}

			pages.Add(page);

			var masterTab = (masterPage as ITdiPage)?.TdiTab;
			if(masterTab == null)
				tdiNotebook.AddTab((page as ITdiPage).TdiTab);
			else if (masterTab?.TabParent is TdiSliderTab slider)
				slider.AddSlaveTab(masterTab, (page as ITdiPage).TdiTab); 
			else
				tdiNotebook.AddSlaveTab(masterTab, (page as ITdiPage).TdiTab);
		}

		protected override void OpenPage(IPage masterPage, IPage page)
		{
			if(page.ViewModel is IWindowDialogSettings) {
				pages.Add(page);
				OpenWindowPage(page);
				return;
			}

			var masterTab = (masterPage as ITdiPage)?.TdiTab;

			if (masterTab is ITdiJournal && masterTab.TabParent is TdiSliderTab && (page.ViewModel as ISlideableViewModel)?.AlwaysNewPage != true) {
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
			if(typeof(TViewModel).IsAssignableTo<IWindowDialogSettings>())
				return viewModelsGtkWindowsFactory;
			else
				return viewModelsFactory;

		}

		#region WindowDialogs

		protected void OpenWindowPage(IPage page)
		{
			var gtkPage = (IGtkWindowPage)page;
			IWindowDialogSettings windowSettings = (IWindowDialogSettings)page.ViewModel;
			gtkPage.GtkView = viewResolver.Resolve(page.ViewModel);
			if(gtkPage.GtkView == null)
				throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");
			gtkPage.GtkDialog = new Gtk.Dialog(gtkPage.ViewModel.Title,
				windowSettings.IsModal ? tdiNotebook.Toplevel as Window : null,
				windowSettings.IsModal ? DialogFlags.Modal : DialogFlags.DestroyWithParent);
			var defaultsize = gtkPage.GtkView.GetType().GetAttribute<WindowSizeAttribute>(true);
			gtkPage.GtkDialog.SetDefaultSize(defaultsize?.DefaultWidth ?? gtkPage.GtkView.WidthRequest, defaultsize?.DefaultHeight ?? gtkPage.GtkView.WidthRequest);
			gtkPage.GtkDialog.VBox.Add(gtkPage.GtkView);
			if(windowSettings.EnableMinimizeMaximize)
				gtkPage.GtkDialog.TypeHint = Gdk.WindowTypeHint.Normal;
			if(!windowSettings.Resizable)
				gtkPage.GtkDialog.Resizable = false;
			gtkPage.GtkView.Show();
			gtkPage.GtkDialog.Show();
			MoveWindow(gtkPage.GtkDialog, windowSettings.WindowPosition);
			gtkPage.GtkDialog.DeleteEvent += GtkDialog_DeleteEvent;
			gtkPage.ViewModel.PropertyChanged += (sender, e) => gtkPage.GtkDialog.Title = gtkPage.ViewModel.Title;
		}

		void GtkDialog_DeleteEvent(object o, DeleteEventArgs args)
		{
			var page = FindPage(args.Event.Window) ?? throw new InvalidOperationException("Закрыто окно которое не зарегистрировано как страницы в навигаторе");
			ClosePage(page, CloseSource.ClosePage);
		}

		private void MoveWindow(Window window, WindowGravity gravity)
		{
			if(gravity == WindowGravity.None)
				return;

			int x = (window.Screen.Width / 2) - (window.Allocation.Width / 2);
			int y = (window.Screen.Height / 2) - (window.Allocation.Height / 2);
			bool isWindows = System.IO.Path.DirectorySeparatorChar == '\\';

			if(gravity.HasFlag(WindowGravity.Left))
				x = 0;
			if(gravity.HasFlag(WindowGravity.Top))
				y = 0;
			if(gravity.HasFlag(WindowGravity.Right))
				x = window.Screen.Width - window.Allocation.Width - (isWindows ? 10 : 0);
			if(gravity.HasFlag(WindowGravity.Bottom))
				y = window.Screen.Height - window.Allocation.Height - (isWindows ? 74 : 0);

			window.Move(x, y);
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
