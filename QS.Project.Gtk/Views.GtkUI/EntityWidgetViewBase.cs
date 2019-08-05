using System;
using QS.ViewModels;
namespace QS.Views.GtkUI
{
	public abstract class EntityWidgetViewBase<TViewModel> : Gtk.Bin
		where TViewModel : UoWWidgetViewModelBase
	{
		protected TViewModel ViewModel { get; }

		protected EntityWidgetViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}
	}
}
