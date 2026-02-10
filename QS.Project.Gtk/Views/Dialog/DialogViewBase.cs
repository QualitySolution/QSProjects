using System;
using QS.Navigation;

namespace QS.Views.Dialog
{
	public abstract class DialogViewBase<TViewModel> : Gtk.Bin
		where TViewModel : IDialogViewModel
	{
		public TViewModel ViewModel { get; private set; }

		protected DialogViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel != null ? viewModel : throw new ArgumentNullException(nameof(viewModel));
		}
	}
}
