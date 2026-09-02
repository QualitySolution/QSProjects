using Gdk;
using Gtk;

namespace QS.Dialog
{
	public class GtkClipboardService : IClipboardService
	{
		public void SetText(string text)
		{
			var clipboard = Clipboard.Get(Atom.Intern("CLIPBOARD", false));
			clipboard.Text = text;
			clipboard.Store();
		}
	}
}
