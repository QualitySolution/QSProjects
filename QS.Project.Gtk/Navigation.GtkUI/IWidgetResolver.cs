using System;
using Gtk;
using QS.ViewModels;

namespace QS.Navigation.GtkUI
{
	public interface IWidgetResolver
	{
		Widget Resolve(ViewModelBase viewModel);
	}
}
