using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views
{
	public partial class ModalDialogView : DialogViewBase<ModalDialogViewModel>
	{
		public ModalDialogView(ModalDialogViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
