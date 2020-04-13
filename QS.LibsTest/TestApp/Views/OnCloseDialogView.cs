using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views
{
	public partial class OnCloseDialogView : DialogViewBase<OnCloseDialogViewModel>
	{
		public OnCloseDialogView(OnCloseDialogViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
