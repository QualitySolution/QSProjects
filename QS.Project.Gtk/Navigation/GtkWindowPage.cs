using Gtk;

namespace QS.Navigation
{
	public class GtkWindowPage<TViewModel> : PageBase, IPage<TViewModel>, IGtkWindowPage
		where TViewModel : IDialogViewModel
	{
		public override string Title => ViewModel?.Title;
		public TViewModel ViewModel { get; private set; }

		IDialogViewModel IPage.ViewModel => ViewModel;

		public Widget GtkView { get; set; }

		public Gtk.Dialog GtkDialog { get; set; }

		public GtkWindowPage(TViewModel viewModel, string hash)
		{
			ViewModel = viewModel;
			PageHash = hash;
		}

	}
}
