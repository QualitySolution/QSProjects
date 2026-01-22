using Avalonia.Controls;
using QS.ViewModels.Dialog;

namespace QS.Navigation;

public class AvaloniaPage<TViewModel> : PageBase, IPage<TViewModel>, IAvaloniaPage where TViewModel : DialogViewModelBase {
	public Control View { get; set; }

	public TViewModel ViewModel { get; private set; }
	DialogViewModelBase IPage.ViewModel => ViewModel;

	public override string Title => ViewModel?.Title;

	public AvaloniaPage(TViewModel viewModel, string hash) {
		ViewModel = viewModel;
		PageHash = hash;
	}
}
