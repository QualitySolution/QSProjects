using System;
using Gdk;
using Gtk;

namespace QSUpdater
{
	public partial class UpdaterDialog : Gtk.Dialog
	{
		public UpdaterDialog (string text, UpdateResult result)
		{
			this.Build ();
			UpdLabel.Markup = text;
			infoLabel.Visible = (result.InfoLink != String.Empty);
			infoLabel.Markup = String.Format ("<b><a href=\" " + result.InfoLink + "\" title=\"Перейти на сайт компании\">Посмотреть полное описание обновления.</a></b>");
			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate {
				System.Diagnostics.Process.Start (result.InfoLink);
			};
			if (!result.HasUpdate) {
				buttonSkip.Visible = buttonOk.Visible = false;
				buttonCancel.Label = "Закрыть";
			}
		}
	}
}

