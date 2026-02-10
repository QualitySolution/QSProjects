using System;

namespace QS.Navigation
{
	public class ViewModelOpenedEventArgs : EventArgs
	{
		public IDialogViewModel ViewModel { get; private set; }
		public IPage Page { get; private set; }

		public ViewModelOpenedEventArgs(IPage page)
		{
			Page = page;
			ViewModel = page.ViewModel;
		}

		public ViewModelOpenedEventArgs(IDialogViewModel viewModel)
		{
			ViewModel = viewModel;
		}
	}
}
