using System;
using Gdk;
using Gtk;
using QS.Launcher.ViewModels;

namespace QS.Launcher.Views {

	public partial class MainWindow : Gtk.Window {
		
		public MainWindowVM ViewModel { get; }
		
		public MainWindow(MainWindowVM viewModel, LauncherOptions options, LoginView loginView, DataBasesView dataBasesView) :
				base(Gtk.WindowType.Toplevel) {
			this.Build();

			Title = options.AppTitle;
			Icon = new Pixbuf(options.LogoIcon);
			ViewModel = viewModel;
			notebookMain.ShowTabs = false;
			notebookMain.AppendPage(
			 	loginView,
				new Label("Выбор подключения")
			);
			loginView.Show();

			notebookMain.AppendPage(
			 	dataBasesView,
			 	new Label("Выбор Базы")
			);
			ViewModel.PagesCount = notebookMain.NPages;
			notebookMain.Binding.AddBinding(ViewModel, v => v.SelectedPageIndex, w => w.Page)
				.InitializeFromSource();
			dataBasesView.Show();
		}
	}
}
