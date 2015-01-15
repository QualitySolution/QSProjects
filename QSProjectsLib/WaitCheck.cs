using System;
using Gtk;
using System.Threading;

namespace QSProjectsLib
{
	public partial class WaitCheck : Gtk.Window
	{
		public WaitCheck () : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}

		public void StartProgressBar(int seconds) {
			double percentInSecond = 1.0 / seconds / 20.0;
			for (int i = 0; i < seconds * 20; i++) {
				progressbar1.Fraction += percentInSecond;
				Thread.Sleep (50);
				QSMain.WaitRedraw ();
			}
		}
	}
}

