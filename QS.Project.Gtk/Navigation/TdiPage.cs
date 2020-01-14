using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation
{
	public class TdiPage<TViewModel> : PageBase, IPage, IPage<TViewModel>, ITdiPage
		where TViewModel : DialogViewModelBase
	{
		public TdiPage(TViewModel viewModel, ITdiTab tab, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
			TdiTab = tab;
		}

		public TViewModel ViewModel { get; private set; }

		public ITdiTab TdiTab { get; private set; }

		DialogViewModelBase IPage.ViewModel => ViewModel;
	}
}
