using QS.Navigation;

namespace QS.Testing.Navigation {
	public class PageForTests<TViewModel> : PageBase, IPage<TViewModel>
		where TViewModel : IDialogViewModel
	{
		public PageForTests(TViewModel viewModel) {
			ViewModel = viewModel;
		}

		public override string Title => ViewModel?.Title;
		public TViewModel ViewModel { get; private set; }
		IDialogViewModel IPage.ViewModel => ViewModel;
	}
}
