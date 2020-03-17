using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views
{
	public partial class SlideableView : DialogViewBase<SlideableViewModel>
	{
		public SlideableView(SlideableViewModel viewModel) : base (viewModel)
		{
			this.Build();
		}
	}
}
