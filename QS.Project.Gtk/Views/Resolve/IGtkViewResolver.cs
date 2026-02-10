using Gtk;

namespace QS.Views.Resolve
{
	public interface IGtkViewResolver
	{
		Widget Resolve(object viewModel);
	}
}
