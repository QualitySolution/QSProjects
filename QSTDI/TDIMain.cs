using System;
using Gtk;

namespace QSTDI
{
	public static class TDIMain
	{
		public static TdiNotebook MainNotebook;

		public static void TDIHandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
		{
			if (MainNotebook == null)
				return;

			int platform = (int)Environment.OSVersion.Platform;
			int version = (int)Environment.OSVersion.Version.Major;
			Gdk.ModifierType modifier;

			//Kind of MacOSX
			if ((platform == 4 || platform == 6 || platform == 128) && version > 8)
				modifier = Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod1Mask;
			//Kind of Windows or Unix
			else
				modifier = Gdk.ModifierType.ControlMask;

			//CTRL+S || CTRL+ENTER
			if ((args.Event.Key == Gdk.Key.S
				|| args.Event.Key == Gdk.Key.s
				|| args.Event.Key == Gdk.Key.Cyrillic_yeru
				|| args.Event.Key == Gdk.Key.Cyrillic_YERU
				|| args.Event.Key == Gdk.Key.Return) && args.Event.State.HasFlag(modifier)) {
				var w = MainNotebook.CurrentPageWidget;
				if (w is TabVBox) {
					var tab = (w as TabVBox).Tab;
					if (tab is TdiSliderTab) {
						var dialog = (tab as TdiSliderTab).ActiveDialog;
						dialog.SaveAndClose ();
					}
					if(tab is ITdiDialog)
					{
						(tab as ITdiDialog).SaveAndClose();
					}
				}
			}

		}

	}
}

