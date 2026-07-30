using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Deletion.ViewModels;
using System;

namespace QS.Deletion.Views;

public partial class DeletionView : UserControl {
	private readonly DeletionViewModel viewModel = null!;

	public DeletionView() {
		InitializeComponent();
	}

	public DeletionView(DeletionViewModel viewModel) {
		InitializeComponent();
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		// в разметке не биндятся
		treeObjects.ItemsSource = viewModel.DeletedItems;
		treeDependence.ItemsSource = viewModel.DependenceTree;
	}

	void OnButtonDeleteClicked(object sender, RoutedEventArgs e) {
		viewModel.RunDetetion();
	}

	void OnButtonCancelClicked(object sender, RoutedEventArgs e) {
		viewModel.CancelDeletion();
	}
}
