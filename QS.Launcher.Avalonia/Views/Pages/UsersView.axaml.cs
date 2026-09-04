using System;
using Avalonia.Controls;
using Avalonia.Input;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;

public partial class UsersView : UserControl {
	private readonly UsersVM viewModel;

	public UsersView(UsersVM viewModel) {
		InitializeComponent();

		DataContext = this.viewModel = viewModel;
	}

	private void Users_OnDoubleTapped(object? sender, TappedEventArgs e) {
		if(users.SelectedItem is not null)
			viewModel.EditUserCommand.Execute().Subscribe();
	}
}
