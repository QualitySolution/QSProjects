using System;
using QS.ViewModels.Dialog;

namespace QS.Views.Dialog
{
	public abstract class DialogViewBase<TViewModel> : Gtk.Bin
		where TViewModel : DialogViewModelBase
	{
		public TViewModel ViewModel { get; private set; }

		protected DialogViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}
	}
}
