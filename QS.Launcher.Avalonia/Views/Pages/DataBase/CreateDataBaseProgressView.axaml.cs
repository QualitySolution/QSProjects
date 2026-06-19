using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Launcher.ViewModels.PageViewModels.DataBase;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class CreateDataBaseProgressView : UserControl {
	public CreateDataBaseProgressView(CreateDataBaseProgressVM progressVM) {
		InitializeComponent();

		DataContext = progressVM;
	}

	private void OnLoaded(object? sender, RoutedEventArgs e) {
		cogwheel.Classes.Add("rolled");

		if(DataContext is CreateDataBaseProgressVM vm)
			vm.StartCommand.Execute().Subscribe();
	}
}
