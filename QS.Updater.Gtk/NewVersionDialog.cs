using System;
using Gdk;
using QSUpdater;

namespace QS.Updater
{
	public partial class NewVersionDialog : Gtk.Dialog
	{
		public NewVersionDialog (string text, UpdateResult result, UpdaterFlags flags)
		{
			this.Build ();
			UpdLabel.WidthChars = 67;
			UpdLabel.Markup = text;
			infoLabel.Visible = (result.InfoLink != String.Empty);
			infoLabel.Markup = String.Format ("<b><a href=\" " + result.InfoLink + "\" title=\"Перейти на сайт компании\">Посмотреть полное описание обновления.</a></b>");
			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate {
				System.Diagnostics.Process.Start (result.InfoLink);
			};
			if (flags.HasFlag(UpdaterFlags.UpdateRequired))
				this.DeleteEvent += delegate {
					Environment.Exit (0);
				};
			buttonSkip.Visible = !flags.HasFlag(UpdaterFlags.UpdateRequired);
			if (flags.HasFlag(UpdaterFlags.UpdateRequired) || !result.HasUpdate)
				buttonCancel.Label = "Закрыть";
			if (!result.HasUpdate)
				buttonSkip.Visible = buttonOk.Visible = false;
		}
	}
}

