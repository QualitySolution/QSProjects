using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using QSSupportLib;
using Gtk;
using QSProjectsLib;
using System.Net;
using System.ComponentModel;
using NLog;

namespace QSUpdater
{
	public class TestUpdate
	{
		public static Logger logger = LogManager.GetCurrentClassLogger ();
		public static IUpdateService svc;
		public static Uri address;
		public static MenuItem updMenu;
		public static ProgressBar updPr;
		public static Window updWin;
		public static UpdateResult res;
		public static string updMessage;
		public static string noties = "Не проверять это обновление";
		public static string tempPath = System.IO.Path.GetTempPath ();
		public static string newVersion;
		public static string checkVersion = String.Empty;
		public static string checkResult = String.Empty;

		static public void LoadUpd ()
		/*{
			Thread loadThread = new Thread (new ThreadStart (ThreadWorks));
			loadThread.Start ();
		}

		static void ThreadWorks ()*/
		{
			address = new Uri ("http://localhost:8080/Updater");
			var factory = new WebChannelFactory<IUpdateService> (new WebHttpBinding { AllowCookies = true }, address);
			svc = factory.CreateChannel ();
			res = svc.CheckUpd (MainSupport.ProjectVerion.Product, MainSupport.ProjectVerion.Edition, "", MainSupport.ProjectVerion.Version.Major, MainSupport.ProjectVerion.Version.Minor, MainSupport.ProjectVerion.Version.Build, MainSupport.ProjectVerion.Version.Revision); 

			try {
				Thread.Sleep (2000);
				Application.Invoke (delegate { 

					newVersion = res.NewVersion;
					if (QSMain.Configsource.Configs ["Updater"] != null) {
						checkVersion = QSMain.Configsource.Configs ["Updater"].Get ("NewVersion", String.Empty);
						checkResult = QSMain.Configsource.Configs ["Updater"].Get ("Check", String.Empty);
					}
					if (res.HasUpdate) {
						if (checkResult == "True" || checkResult == String.Empty) {
							ShowDialog ();
						} else if (checkVersion != newVersion)
							ShowDialog ();
					} else
						ShowDialog ();
				}
				);
			} catch (Exception ee) {
				logger.ErrorException ("Ошибка доступа к серверу. Возможно сервер не доступен.", ee);
				Console.ReadLine ();
			}
		}

		public static void ShowDialog ()
		{
			if (res.HasUpdate) {
				updMessage = string.Format ("<b>Доступна новая версия программы " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
				"\n\nВаша версия: " + MainSupport.ProjectVerion.Version +
				"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");
				    
				if (res.UpdateDescription != String.Empty)
					updMessage = string.Format ("<b>Доступна новая версия программы: " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
					"\n\nВаша версия программы: " + MainSupport.ProjectVerion.Version + "\n\nИнформация об обновлении:\n" + res.UpdateDescription +
					"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");

				/*if (res.InfoLink != String.Empty)
					updMessage = string.Format ("<b>Доступна новая версия программы: " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
					"\n\nВаша версия программы: " + MainSupport.ProjectVerion.Version + "\n\nИнформация об обновлении:\n" + res.UpdateDescription +
					"\n\n<b><a href=\" " + res.InfoLink + "\" title=\"Перейти на сайт компании\">Что нового?</a></b>\n" +
					"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");*/

			} else
				updMessage = string.Format ("<b>Ваша версия: " + MainSupport.ProjectVerion.Version + "</b>" +
				"\n\nНа данный момент это актуальная версия продукта.");


			UpdaterDialog updDlg = new UpdaterDialog ();
			updDlg.Resizable = false;
			int result = updDlg.Run ();
			updDlg.Destroy ();

			if ((ResponseType)result == ResponseType.Ok) {
				updWin = new Window ("Подождите...");
				updWin.SetSizeRequest (300, 25); 
				updWin.Resizable = false;
				updWin.SetPosition (WindowPosition.Center);
				updPr = new ProgressBar ();
				VBox vbox = new VBox ();
				updWin.Add (vbox); 
				updPr.Text = "Новая версия скачивается, подождите...";
				vbox.PackStart (updPr, true, true, 0);

				updWin.ShowAll ();

				WebClient webClient = new WebClient ();
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler (Completed);
				webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (ProgressChanged);
				webClient.DownloadFileAsync (new Uri (res.FileLink), tempPath + @"\QSInstaller.exe");

				if (QSMain.Configsource.Configs ["Updater"] == null) {
					QSMain.Configsource.AddConfig ("Updater");
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				} else {
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				}

			} else if ((ResponseType)result == ResponseType.Cancel)
				ConfigFileUpdater ();
		}

		protected static void ConfigFileUpdater ()
		{
			if (UpdaterDialog.updChecker) {

				if (QSMain.Configsource.Configs ["Updater"] == null) {
					QSMain.Configsource.AddConfig ("Updater");
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				} else {
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				}
			} else {
				if (QSMain.Configsource.Configs ["Updater"] == null) {
					QSMain.Configsource.AddConfig ("Updater");
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "True");
					QSMain.Configsource.Save ();
				} else {
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "True");
					QSMain.Configsource.Save ();
				}
			}
		}

		public static void ProgressChanged (object sender, DownloadProgressChangedEventArgs e)
		{
			Application.Invoke (delegate {
				updPr.Fraction = e.ProgressPercentage / 100.0;
			});
		}

		public static void Completed (object sender, AsyncCompletedEventArgs e)
		{
			if (updWin.IsMapped) {
				StartProcess ();
				Application.Quit ();
			}
		}

		public static void StartProcess ()
		{
			System.Diagnostics.Process File = new System.Diagnostics.Process ();
			File.StartInfo.FileName = tempPath + @"\QSInstaller.exe";
			updWin.Destroy ();
			File.Start ();
		}
	}
}

