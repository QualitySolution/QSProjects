using Avalonia.Controls;
using QS.ViewModels.Dialog;

namespace QS.Navigation;

public class AvaloniaPage<TViewModel> : PageBase, IPage<TViewModel>, IAvaloniaPage 
	where TViewModel : IDialogViewModel 
{
	public Control View { get; set; }

	public TViewModel ViewModel { get; private set; }
	IDialogViewModel IPage.ViewModel => ViewModel;

	public override string Title => ViewModel?.Title;

	public AvaloniaPage(TViewModel viewModel, string hash) {
		ViewModel = viewModel;
		PageHash = hash;
	}
}
