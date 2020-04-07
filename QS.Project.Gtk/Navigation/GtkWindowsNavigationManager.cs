using System;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Views.Dialog;
using QS.Views.Resolve;

namespace QS.Navigation
{
	public class GtkWindowsNavigationManager : NavigationManagerBase, INavigationManager
	{
		readonly IGtkViewResolver viewResolver;
		protected readonly IViewModelsPageFactory viewModelsFactory;

		public GtkWindowsNavigationManager(IViewModelsPageFactory viewModelsFactory, IInteractiveMessage interactive, IGtkViewResolver viewResolver, IPageHashGenerator hashGenerator = null)
			: base(interactive, hashGenerator)
		{
			this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
			this.viewResolver = viewResolver;
		}

		#region Закрытие

		public bool AskClosePage(IPage page)
		{
			throw new  NotImplementedException();
		}

		public void ForceClosePage(IPage page)
		{
			ClosePage(page);
			(page as IGtkWindowPage).GtkDialog.Respond((int)ResponseType.DeleteEvent); 
			(page as IGtkWindowPage).GtkView.Destroy();
			(page as IGtkWindowPage).GtkDialog.Destroy();
		}

		#endregion

		public override void SwitchOn(IPage page)
		{
			throw new NotImplementedException();
		}

		protected override void OpenSlavePage(IPage masterPage, IPage page)
		{
			OpenPage(masterPage, page);
		}

		protected override void OpenPage(IPage masterPage, IPage page)
		{
			//FIXME Временное решение пока не выпилим TDi, и реализуем заполнение этого через конструктор.
			page.ViewModel.NavigationManager = this;
			pages.Add(page);
			var gtkPage = (IGtkWindowPage)page;
			var gtkMasterPage = (IGtkWindowPage)masterPage;
			gtkPage.GtkView = viewResolver.Resolve(page.ViewModel);
			if(gtkPage.GtkView == null)
				throw new InvalidOperationException($"View для {page.ViewModel.GetType()} не создано через {viewResolver.GetType()}.");
			gtkPage.GtkDialog = new Gtk.Dialog(gtkPage.ViewModel.Title, gtkMasterPage?.GtkDialog, DialogFlags.Modal);
			var defaultsize = gtkPage.GtkView.GetType().GetAttribute<WindowSizeAttribute>(true);
			gtkPage.GtkDialog.SetDefaultSize(defaultsize?.DefaultWidth ?? gtkPage.GtkView.WidthRequest, defaultsize?.DefaultHeight ?? gtkPage.GtkView.WidthRequest);
			gtkPage.GtkDialog.VBox.Add(gtkPage.GtkView);
			gtkPage.GtkView.Show();
			gtkPage.GtkDialog.Show();
			gtkPage.GtkDialog.DeleteEvent += GtkDialog_DeleteEvent;
			gtkPage.ViewModel.PropertyChanged += (sender, e) => gtkPage.GtkDialog.Title = gtkPage.ViewModel.Title;
		}

		public IPage FindPage(Gdk.Window window)
		{
			return AllPages.Cast<IGtkWindowPage>().FirstOrDefault(x => x.GtkDialog.GdkWindow == window);
		}

		void GtkDialog_DeleteEvent(object o, DeleteEventArgs args)
		{
			var page = FindPage(args.Event.Window) ?? throw new InvalidOperationException("Закрыто окно которое не зарегистрировано как страницы в навигаторе");
			ForceClosePage(page);
		}

		protected override IViewModelsPageFactory GetPageFactory<TViewModel>()
		{
			return viewModelsFactory;
		}
	}
}
