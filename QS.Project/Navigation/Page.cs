using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public class Page<TViewModel> : IPage<TViewModel>, IPageInternal
		where TViewModel : ViewModelBase
	{
		public Page(TViewModel viewModel, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
		}

		public TViewModel ViewModel { get; private set; }

		ViewModelBase IPage.ViewModel => ViewModel;

		public event EventHandler PageClosed;

		public string PageHash { get; private set; }

		public List<IPage> SlavePages { get; } = new List<IPage>();

		public List<IPage> ChildPages { get; } = new List<IPage>();

		void IPageInternal.OnClosed()
		{
			PageClosed?.Invoke(this, EventArgs.Empty);
		}
	}
}
