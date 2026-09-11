using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Launcher.ViewModels.PageViewModels.Database;

namespace QS.Launcher.Views.Pages.Database;

public partial class CreateDatabaseProgressView : UserControl {
	public CreateDatabaseProgressView(CreateDatabaseProgressVM progressVM) {
		InitializeComponent();

		DataContext = progressVM;
	}

	private void OnLoaded(object? sender, RoutedEventArgs e) {
		if(DataContext is CreateDatabaseProgressVM vm)
			vm.StartCommand.Execute().Subscribe();
	}
}
