using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using QS.Deletion.ViewModels;
using System;

namespace QS.Deletion.Views;

public partial class DeletionProcessView : UserControl {
	private readonly DeletionProcessViewModel viewModel = null!;

	public DeletionProcessView() {
		InitializeComponent();
	}

	public DeletionProcessView(DeletionProcessViewModel viewModel) {
		InitializeComponent();
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		viewModel.Deletion.PropertyChanged += (s, e) => Dispatcher.UIThread.Post(UpdateProgress);
		UpdateProgress();
	}

	void UpdateProgress() {
		textOperation.Text = viewModel.Deletion.OperationTitle;
		progressOperation.Maximum = viewModel.Deletion.ProgressUpper > 0 ? viewModel.Deletion.ProgressUpper : 1;
		progressOperation.Value = viewModel.Deletion.ProgressValue;
	}

	void OnButtonCancelClicked(object sender, RoutedEventArgs e) {
		viewModel.CancelOperation();
	}
}
