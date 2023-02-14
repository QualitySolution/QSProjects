using System;
using QS.ViewModels;

namespace QS.Navigation
{
	public class ViewModelOpenedEventArgs : EventArgs
	{
		public ViewModelBase ViewModel { get; private set; }
		public IPage Page { get; private set; }

		public ViewModelOpenedEventArgs(IPage page)
		{
			Page = page;
			ViewModel = page.ViewModel;
		}

		public ViewModelOpenedEventArgs(ViewModelBase viewModel)
		{
			ViewModel = viewModel;
		}
	}
}
