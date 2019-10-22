using System;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Views.GtkUI
{
	public abstract class TabViewBase<TViewModel> : TabViewBase
		where TViewModel : TabViewModelBase
	{
		public TViewModel ViewModel { get; set; }

		public sealed override ITdiTab Tab => ViewModel;

		public TabViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}

		public override void Destroy()
		{
			ViewModel?.Dispose();
			base.Destroy();
		}
	}
}
