using System;
using Gtk;
using QS.Views.Resolve;

namespace QS.Navigation
{
	public class GtkWindowsNavigationManager : NavigationManagerBase, INavigationManager
	{
		readonly IGtkViewResolver viewResolver;

		public GtkWindowsNavigationManager(IPageHashGenerator hashGenerator, IViewModelsPageFactory viewModelsFactory, IGtkViewResolver viewResolver)
			: base(hashGenerator, viewModelsFactory)
		{
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
			gtkPage.GtkDialog = new Gtk.Dialog(gtkPage.ViewModel.Title, gtkMasterPage?.GtkDialog, DialogFlags.Modal);
			gtkPage.GtkDialog.SetDefaultSize(800, 500);
			gtkPage.GtkDialog.VBox.Add(gtkPage.GtkView);
			gtkPage.GtkView.Show();
			gtkPage.GtkDialog.Show();
			gtkPage.GtkDialog.Run();
			gtkPage.GtkDialog.Destroy();
		}
	}
}
