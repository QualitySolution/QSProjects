using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views
{
	public partial class DialogWithEntityUoWBuilderView : DialogViewBase<DialogWithEntityUoWBuilderViewModel>
	{
		public DialogWithEntityUoWBuilderView(DialogWithEntityUoWBuilderViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
