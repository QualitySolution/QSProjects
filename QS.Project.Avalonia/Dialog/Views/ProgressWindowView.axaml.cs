using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Dialog.ViewModels;
using QS.Widgets;

namespace QS.Dialog.Views;

public partial class ProgressWindowView : UserControl {
	private readonly ProgressWindowViewModel? viewModel;

	public ProgressWindowView() {
		InitializeComponent();
	}

	public ProgressWindowView(ProgressWindowViewModel viewModel, IGuiDispatcher guiDispatcher) : this() {
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		DataContext = viewModel;

		var widget = new ProgressWidget(guiDispatcher);
		viewModel.Progress = widget;
		progressPlaceholder.Content = widget;
	}

	private void OnCancelClicked(object? sender, RoutedEventArgs e) => viewModel?.CancellationTokenSource?.Cancel();
}
