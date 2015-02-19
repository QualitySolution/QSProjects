using System;
using Gdk;
using Gtk;

namespace QSUpdater
{
	public partial class UpdaterDialog : Gtk.Dialog
	{
		public static bool updChecker;
		public static string checkVersion = String.Empty;

		public UpdaterDialog ()
		{
			this.Build ();
			UpdLabel.Markup = TestUpdate.updMessage;
			checkVersion = TestUpdate.res.NewVersion;
			this.Destroyed += HandleDestroyed;
			infoLabel.Visible = (TestUpdate.res.InfoLink != String.Empty);
			infoLabel.Markup = String.Format ("<b><a href=\" " + TestUpdate.res.InfoLink + "\" title=\"Перейти на сайт компании\">Что нового?</a></b>");
			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate (object o, ButtonPressEventArgs args) {
				System.Diagnostics.Process.Start (TestUpdate.res.InfoLink);
			};
				
			Updcb.Active = (TestUpdate.checkResult == "False" && TestUpdate.checkVersion == TestUpdate.res.NewVersion);

			if (!TestUpdate.res.HasUpdate) {
				Updcb.Visible = buttonOk.Visible = false;
				buttonCancel.Label = "Закрыть";
			}
		}

		void HandleDestroyed (object sender, EventArgs e)
		{
			updChecker = Updcb.Active;
		}
	}
}

