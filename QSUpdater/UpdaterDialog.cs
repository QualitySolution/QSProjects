using System;
using Gdk;
using Gtk;

namespace QSUpdater

{
	public partial class UpdaterDialog : Gtk.Dialog
	{
		public static bool updChecker;
		public static string checkVersion = "";

		public UpdaterDialog ()
		{
			this.Build ();
			UpdLabel.Markup = TestUpdate.updMessage;
			Updcb.Label = TestUpdate.noties;
			checkVersion = TestUpdate.res.NewVersion;
			this.Destroyed += HandleDestroyed;
			if (TestUpdate.res.InfoLink != String.Empty)
				infoLabel.Visible = true;
			infoLabel.Markup = String.Format ("<b><a href=\" " +TestUpdate.res.InfoLink + "\" title=\"Перейти на сайт компании\">Что нового?</a></b>");
			//	infoLabel.Markup = String.Format ("<b><u><span foreground='#0000FF'>Что нового?</span></u></b>");
			//	infoLabel.Markup = String.Format ("<b><a>Что нового?</a></b>");

			infoLabel.AddEvents ((int)EventMask.ButtonPressMask);
			infoLabel.ButtonPressEvent += delegate (object o, ButtonPressEventArgs args) {
				System.Diagnostics.Process.Start(TestUpdate.res.InfoLink);
			};

			/*infoLabel.AddEvents ((int)EventMask.AllEventsMask);
			*infoLabel.FocusInEvent += delegate(object o, FocusInEventArgs args) {
				infoLabel.Markup = String.Format ("<b><u><span foreground='#0000FF' background='ADD8E6' >Что нового?</span></u></b>");
			};*/
			//updChecker = Updcb.Active;

			if (!TestUpdate.res.HasUpdate) {
				Updcb.Visible = false;
				buttonCancle.Visible = false;
				buttonCancle.Label = "Закрыть";
			}
		}

		void HandleDestroyed (object sender, EventArgs e)
		{
			updChecker = Updcb.Active;
		}

		protected void OnUpdLabelMoveCursor (object o, Gtk.MoveCursorArgs args)
		{
			throw new NotImplementedException ();
		}
	}
}

