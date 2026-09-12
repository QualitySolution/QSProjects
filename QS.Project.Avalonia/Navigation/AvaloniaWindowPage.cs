using Avalonia.Controls;

namespace QS.Navigation;
public class AvaloniaWindowPage<TViewModel> : PageBase, IPage<TViewModel>, IAvaloniaWindowPage
	where TViewModel : IDialogViewModel {
	public Control View { get; set; } = null!;
	public Window? Window { get; set; }

	public TViewModel ViewModel { get; private set; }
	IDialogViewModel IPage.ViewModel => ViewModel;

	public override string Title => ViewModel.Title;

	public AvaloniaWindowPage(TViewModel viewModel, string hash) {
		ViewModel = viewModel;
		PageHash = hash;
	}
}
