using Avalonia.Controls;
using QS.ViewModels.Dialog;
using System;

namespace QS.Navigation;

public class AvaloniaPage<TViewModel> : PageBase, IPage<TViewModel>, IAvaloniaPage where TViewModel : DialogViewModelBase {
	public Control AvaloniaView { get; set; }

	public TViewModel ViewModel { get; private set; }
	DialogViewModelBase IPage.ViewModel => ViewModel;

	public override string Title => throw new NotImplementedException();

	public AvaloniaPage(TViewModel viewModel, string hash) {
		ViewModel = viewModel;
		PageHash = hash;
	}
}
