using System;
using QS.ViewModels;

namespace QS.Views
{
	public abstract class ViewBase<TViewModel> : Gtk.Bin
		where TViewModel : ViewModelBase
	{
		public TViewModel ViewModel { get; private set; }

		protected ViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}
	}
}
