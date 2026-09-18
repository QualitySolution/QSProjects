using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Dialog.ViewModels;

namespace QS.Dialog.Views;

public partial class ProgressWindowView : UserControl {
	private readonly ProgressWindowViewModel? viewModel;

	public ProgressWindowView() {
		InitializeComponent();
	}

	public ProgressWindowView(ProgressWindowViewModel viewModel) : this() {
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		DataContext = viewModel;
		viewModel.Progress = progressWidget;
	}

	private void OnCancelClicked(object? sender, RoutedEventArgs e) => viewModel?.CancellationTokenSource?.Cancel();
}
