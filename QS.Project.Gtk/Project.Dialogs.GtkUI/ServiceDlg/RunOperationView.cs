using System;
using System.Threading;
using Gtk;

namespace QS.Project.Dialogs.GtkUI.ServiceDlg
{
	public partial class RunOperationView : Gtk.Window, IRunOperationView
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public RunOperationView() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			this.Deletable = false;
		}

		public bool StartProgressBar(Thread thread, int seconds)
		{
			progressbar1.Adjustment.Upper = seconds;
			double percentInSecond = 1.0 / 20.0;
			for(int i = 0; i < seconds * 20; i++) {
				if(!thread.IsAlive)
					return false;
				progressbar1.Adjustment.Value += percentInSecond;
				Thread.Sleep(50);
				while(Application.EventsPending()) {
					Gtk.Main.Iteration();
				}
			}
			if(thread.IsAlive)
				thread.Abort();
			logger.Warn("Таймаут ожидания потока с проверкой...");
			return true;
		}

		public bool RunOperation(ThreadStart function, int timeout, string text = "")
		{
			bool timeOutReached = false;

			Thread thread = new Thread(function);
			thread.Start();
			//Ждем 1 секунду
			thread.Join(1000);
			//Если секунда прошла, а операция еще не закончилась - показываем окно
			if(thread.IsAlive) {
				if(text != "")
					labelMsg.LabelProp = text;
				Show();
				timeOutReached = StartProgressBar(thread, timeout);
			}
			Destroy();
			return timeOutReached;
		}
	}
}
