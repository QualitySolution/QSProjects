using Gtk;

namespace QS.Navigation
{
	public interface IGtkWindowPage : IPage
	{
		Widget GtkView { get; set; }

		Gtk.Dialog GtkDialog { get; set; }
	}
}
