using System;
using Gtk;
using QS.Launcher.ViewModels;

namespace QS.Launcher.Views {

	public partial class MainWindow : Gtk.Window {
		
		public MainWindowVM ViewModel { get; }
		
		public MainWindow(MainWindowVM viewModel, LoginView loginView, DataBasesView dataBasesView) :
				base(Gtk.WindowType.Toplevel) {
			this.Build();

			ViewModel = viewModel;
			 notebookMain.AppendPage(
			 	loginView,
				new Label("Выбор подключения")
			 );
			loginView.Show();

			 notebookMain.AppendPage(
			 	dataBasesView,
			 	new Label("Выбор Базы")
			 );
			dataBasesView.Show();
		}
	}
}
