using System;
using Gdk;
using Gtk;

namespace QSUpdater
{
	public partial class UpdaterDialog : Gtk.Dialog
	{
		public static bool updChecker;
		public static string checkVersion = String.Empty;

		public UpdaterDialog (string text)
		{
			this.Build ();
			UpdLabel.Markup = text;
			checkVersion = CheckUpdate.res.NewVersion;
			infoLabel.Visible = (CheckUpdate.res.InfoLink != String.Empty);
			infoLabel.Markup = String.Format ("<b><a href=\" " + CheckUpdate.res.InfoLink + "\" title=\"Перейти на сайт компании\">Посмотреть полное описание обновления.</a></b>");
			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate (object o, ButtonPressEventArgs args) {
				System.Diagnostics.Process.Start (CheckUpdate.res.InfoLink);
			};
				
			//Updcb.Active = (CheckUpdate.checkResult == "False" && CheckUpdate.checkVersion == CheckUpdate.res.NewVersion);

			if (!CheckUpdate.res.HasUpdate) {
				buttonSkip.Visible = buttonOk.Visible = false;
				buttonCancel.Label = "Закрыть";
			}
		}
	}
}

