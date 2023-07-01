using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Testing.Testing.Navigation {
	public class PageForTests<TViewModel> : PageBase, IPage<TViewModel>
		where TViewModel : DialogViewModelBase
	{
		public PageForTests(TViewModel viewModel) {
			ViewModel = viewModel;
		}

		public override string Title => ViewModel?.Title;
		public TViewModel ViewModel { get; private set; }
		DialogViewModelBase IPage.ViewModel => ViewModel;
	}
}
