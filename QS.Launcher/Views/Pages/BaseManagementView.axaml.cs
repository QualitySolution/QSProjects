using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.ViewModels.PageViewModels;
using System;
using System.Linq;

namespace QS.Launcher.Views.Pages;

public partial class BaseManagementView : UserControl {
	public BaseManagementView(BaseManagementVM viewModel) {
		InitializeComponent();
		DataContext = viewModel;
	}
}
