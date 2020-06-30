using System;
using NSubstitute;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

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
