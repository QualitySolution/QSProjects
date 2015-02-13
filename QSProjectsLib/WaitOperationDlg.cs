using System;
using Gtk;
using System.Threading;

namespace QSProjectsLib
{
	public partial class WaitOperationDlg : Gtk.Window
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public WaitOperationDlg () : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.Deletable = false;
		}

		public bool StartProgressBar(Thread thread, int seconds) {
			progressbar1.Adjustment.Upper = seconds;
			double percentInSecond = 1.0 / 20.0;
			for (int i = 0; i < seconds * 20; i++) {
				if (!thread.IsAlive)
					return false;
				progressbar1.Adjustment.Value += percentInSecond;
				Thread.Sleep (50);
				QSMain.WaitRedraw ();
			}
			if (thread.IsAlive)
				thread.Abort ();
			logger.Warn ("Таймаут ожидания потока с проверкой...");
			return true;
		}

		public static bool RunOperationWithDlg(ThreadStart function, int timeout, string text = "")
		{
			WaitOperationDlg wait = null;
			bool timeOutReached = false;

			Thread thread = new Thread (function);
			thread.Start ();
			thread.Join (1000); //Ждем 1 секунду
			if (thread.IsAlive) {							//Если секунда прошла, а пинг еще не закончился - показываем окно
				wait = new WaitOperationDlg ();
				if (text != "")
					wait.labelMsg.LabelProp = text;
				wait.Show ();
				timeOutReached = wait.StartProgressBar (thread, timeout);
			}
			if (wait != null)
				wait.Destroy ();
			return timeOutReached;
		}
	}
}

