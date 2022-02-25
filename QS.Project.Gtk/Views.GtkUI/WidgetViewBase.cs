using System;
using QS.ViewModels;
namespace QS.Views.GtkUI
{
	public abstract class WidgetViewBase<TViewModel> : Gtk.Bin
		where TViewModel : WidgetViewModelBase
	{
		private TViewModel _viewModel;
		public virtual TViewModel ViewModel {
			get => _viewModel;
			set {
				_viewModel = value;
				ConfigureWidget();
			}
		}

		protected WidgetViewBase(TViewModel viewModel)
		{
			_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}

		protected WidgetViewBase() { }

		protected virtual void ConfigureWidget() => Sensitive = ViewModel != null;
	}
}
