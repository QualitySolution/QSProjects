using System;
using Gtk;

namespace QSProjectsLib
{
	public static class ClipboardWorkaround
	{
		private static Gtk.Clipboard clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));

		public static void HandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
		{
			int platform = (int)Environment.OSVersion.Platform;
			int version = (int)Environment.OSVersion.Version.Major;
			Gdk.ModifierType modifier;

			//Kind of MacOSX
			if ((platform == 4 || platform == 6 || platform == 128) && version > 8)
				modifier = Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod1Mask;
			//Kind of Windows or Unix
			else
				modifier = Gdk.ModifierType.ControlMask;

			//CTRL+C	
			if ((args.Event.Key == Gdk.Key.Cyrillic_es || args.Event.Key == Gdk.Key.Cyrillic_ES) && args.Event.State.HasFlag(modifier)) {
				Widget w = (o as Window).Focus;
				CopyToClipboard (w);
			}//CTRL+X
			else if ((args.Event.Key == Gdk.Key.Cyrillic_che || args.Event.Key == Gdk.Key.Cyrillic_CHE) && args.Event.State.HasFlag(modifier)) {
				Widget w = (o as Window).Focus;
				CutToClipboard (w);
			}//CTRL+V
			else if ((args.Event.Key == Gdk.Key.Cyrillic_em || args.Event.Key == Gdk.Key.Cyrillic_EM) && args.Event.State.HasFlag(modifier)) {
				Widget w = (o as Window).Focus;
				PasteFromClipboard (w);
			}
		}

		static void CopyToClipboard (Widget w)
		{
			int start, end;

			if (w is Editable)
				(w as Editable).CopyClipboard ();
			else if (w is TextView)
				(w as TextView).Buffer.CopyClipboard (clipboard);
			else if (w is Label) {
				(w as Label).GetSelectionBounds (out start, out end);
				if (start != end)
					clipboard.Text = (w as Label).Text.Substring (start, end - start);
			}
		}

		static void CutToClipboard (Widget w)
		{
			int start, end;

			if (w is Editable)
				(w as Editable).CutClipboard ();
			else if (w is TextView)
				(w as TextView).Buffer.CutClipboard (clipboard, true);
			else if (w is Label) {
				(w as Label).GetSelectionBounds (out start, out end);
				if (start != end)
					clipboard.Text = (w as Label).Text.Substring (start, end - start);
			}
		}

		static void PasteFromClipboard (Widget w)
		{
			if (w is Editable)
				(w as Editable).PasteClipboard ();
			else if (w is TextView)
				(w as TextView).Buffer.PasteClipboard (clipboard);
		}
	}
}

