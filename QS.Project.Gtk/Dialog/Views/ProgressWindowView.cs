using System;
using QS.Dialog.ViewModels;
using QS.Views;

namespace QS.Dialog.Views
{
	public partial class ProgressWindowView : ViewBase<ProgressWindowViewModel>
	{
		public ProgressWindowView(ProgressWindowViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ViewModel.Progress = progresswidget1;
		}
	}
}
