using System;
using Gdk;
using Gtk;

namespace QSUpdater
{
	public partial class UpdaterDialog : Gtk.Dialog
	{
		public UpdaterDialog (string text, UpdateResult result, bool updateRequired)
		{
			this.Build ();
			UpdLabel.Markup = text;
			infoLabel.Visible = (result.InfoLink != String.Empty);
			infoLabel.Markup = String.Format ("<b><a href=\" " + result.InfoLink + "\" title=\"Перейти на сайт компании\">Посмотреть полное описание обновления.</a></b>");
			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate {
				System.Diagnostics.Process.Start (result.InfoLink);
			};
			if (updateRequired)
				this.DeleteEvent += delegate {
					Environment.Exit (0);
				};
			buttonSkip.Visible = !updateRequired;
			if (updateRequired || !result.HasUpdate)
				buttonCancel.Label = "Закрыть";
			if (!result.HasUpdate)
				buttonSkip.Visible = buttonOk.Visible = false;
		}
	}
}

