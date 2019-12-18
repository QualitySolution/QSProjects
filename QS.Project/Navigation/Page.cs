using System;
using System.Collections.Generic;
using System.Linq;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation
{
	public class Page<TViewModel> : PageBase, IPage, IPage<TViewModel>, ITdiPage
		where TViewModel : ViewModelBase
	{
		public Page(TViewModel viewModel, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
		}

		public TViewModel ViewModel { get; private set; }

		public ITdiTab TdiTab => ViewModel as ITdiTab;

		ViewModelBase IPage.ViewModel => ViewModel;
	}
}
