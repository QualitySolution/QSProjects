using System;
using QS.ViewModels;
namespace QS.Views.GtkUI
{
	public abstract class EntityWidgetViewBase<TViewModel> : Gtk.Bin
		where TViewModel : UoWWidgetViewModelBase
	{
		TViewModel viewModel;
		public TViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				ConfigureWidget();
			}
		}

		protected EntityWidgetViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}

		protected EntityWidgetViewBase() { }

		protected virtual void ConfigureWidget() => Sensitive = ViewModel != null;
	}
}
