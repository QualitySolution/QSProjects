using Gtk;
using QS.ViewModels;

namespace QS.Views.Resolve
{
	public interface IGtkViewResolver
	{
		Widget Resolve(ViewModelBase viewModel);
	}
}
