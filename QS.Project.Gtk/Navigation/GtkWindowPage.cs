using Gtk;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public class GtkWindowPage<TViewModel> : PageBase, IPage<TViewModel>, IGtkWindowPage
		where TViewModel : DialogViewModelBase
	{
		public override string Title => ViewModel?.Title;
		public TViewModel ViewModel { get; private set; }

		DialogViewModelBase IPage.ViewModel => ViewModel;

		public Widget GtkView { get; set; }

		public Gtk.Dialog GtkDialog { get; set; }

		public GtkWindowPage(TViewModel viewModel, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
		}

	}
}
