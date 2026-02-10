using QS.Tdi;

namespace QS.Navigation
{
	public class TdiPage<TViewModel> : PageBase, IPage, IPage<TViewModel>, ITdiPage
		where TViewModel : IDialogViewModel
	{
		public TdiPage(TViewModel viewModel, ITdiTab tab, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
			TdiTab = tab;
		}

		public override string Title => ViewModel?.Title;
		public TViewModel ViewModel { get; private set; }

		public ITdiTab TdiTab { get; private set; }

		IDialogViewModel IPage.ViewModel => ViewModel;
	}
}
