using QS.Test.TestApp.ViewModels;

namespace QS.Test.TestApp.Views
{
	public partial class EmptyDialogView : Gtk.Bin
	{
		public EmptyDialogView()
		{
			this.Build();
		}

		public EmptyDialogView(EmptyDialogViewModel viewModel)
		{
			this.Build();
		}
	}
}
