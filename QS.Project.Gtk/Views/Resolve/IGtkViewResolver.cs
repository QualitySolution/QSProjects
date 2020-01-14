using Gtk;
using QS.ViewModels.Dialog;

namespace QS.Views.Resolve
{
	public interface IGtkViewResolver
	{
		Widget Resolve(DialogViewModelBase viewModel);
	}
}
