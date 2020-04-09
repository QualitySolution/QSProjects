using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views
{
	public partial class OnCloseModalDialogView : DialogViewBase<OnCloseModalDialogViewModel>
	{
		public OnCloseModalDialogView(OnCloseModalDialogViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
