using System;
using QS.ViewModels;
namespace QS.Views.GtkUI
{
	public abstract class WidgetViewBase<TViewModel> : Gtk.Bin
		where TViewModel : WidgetViewModelBase
	{
		TViewModel viewModel;
		public TViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				ConfigureWidget();
			}
		}

		protected WidgetViewBase(TViewModel viewModel)
		{
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}

		protected WidgetViewBase() { }

		protected virtual void ConfigureWidget() => Sensitive = ViewModel != null;
	}
}
