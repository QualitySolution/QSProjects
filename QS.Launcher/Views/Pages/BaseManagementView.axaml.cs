using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QS.Launcher.ViewModels.PageViewModels;
using System;
using System.Windows.Input;

namespace QS.Launcher.Views.Pages;

public partial class BaseManagementView : UserControl {
	public BaseManagementView(BaseMagamenetVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
