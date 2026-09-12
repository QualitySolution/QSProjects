using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using QS.Deletion.ViewModels;
using System;

namespace QS.Deletion.Views;

public partial class PrepareDeletionView : UserControl {
	private readonly PrepareDeletionViewModel viewModel = null!;

	public PrepareDeletionView() {
		InitializeComponent();
	}

	public PrepareDeletionView(PrepareDeletionViewModel viewModel) {
		InitializeComponent();
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		viewModel.Deletion.PropertyChanged += (s, e) => Dispatcher.UIThread.Post(UpdateCounters);
		UpdateCounters();
	}

	void UpdateCounters() {
		textOperation.Text = viewModel.Deletion.OperationTitle;
		textToDelete.Text = viewModel.Deletion.ItemsToDelete.ToString();
		textToClean.Text = viewModel.Deletion.ItemsToClean.ToString();
		textToRemoveFrom.Text = viewModel.Deletion.ItemsToRemoveFrom.ToString();
		textLinks.Text = viewModel.Deletion.TotalLinks.ToString();
	}

	void OnButtonCancelClicked(object sender, RoutedEventArgs e) {
		viewModel.CancelOperation();
	}
}
