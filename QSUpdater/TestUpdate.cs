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
		public static UpdateResult res;
		public static MenuItem updMenu;
		public static Window updWin;
		public static string updMessage, checkVersion, checkResult, serialNumber, newVersion;

		static string tempPath = System.IO.Path.GetTempPath ();
		static ProgressBar updPr;

		static public void LoadUpd ()
		{
			Thread loadThread = new Thread (new ThreadStart (ThreadWorks));
			loadThread.Start ();
		}

		static void ThreadWorks ()
		{
			try {
				logger.Info ("Получаем данные от сервера");
				address = new Uri ("http://saas.qsolution.ru:2048/Updater");
				var factory = new WebChannelFactory<IUpdateService> (new WebHttpBinding { AllowCookies = true }, address);
				svc = factory.CreateChannel ();
				res = svc.checkUpdate (MainSupport.ProjectVerion.Product, MainSupport.ProjectVerion.Edition, serialNumber, MainSupport.ProjectVerion.Version.Major, MainSupport.ProjectVerion.Version.Minor, MainSupport.ProjectVerion.Version.Build, MainSupport.ProjectVerion.Version.Revision); 

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
					} 
				}
				);
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
			}
		}

		public static void ShowDialog ()
		{
			try {
				if (res.HasUpdate) {
					updMessage = string.Format ("<b>\nДоступна новая версия программы " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
					"\n\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + "." +
					"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");
				    
					if (res.UpdateDescription != String.Empty)
						updMessage = string.Format ("<b>\nДоступна новая версия программы: " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
						"\n\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + ".\n\nИнформация об обновлении:\n" + res.UpdateDescription +
						"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");

				} else
					updMessage = string.Format ("<b>\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + ".</b>" +
					"\n\nНа данный момент это актуальная версия продукта.\n");

				UpdaterDialog updDlg = new UpdaterDialog ();
				updDlg.Resizable = false;
				int result = updDlg.Run ();
				updDlg.Destroy ();

				if ((ResponseType)result == ResponseType.Ok) {
					logger.Info ("Скачивание обновления началось.");
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

				} else if ((ResponseType)result == ResponseType.Cancel)
				if (res.HasUpdate)
					ConfigFileUpdater ();
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
				updMessage = "Извините, сервер обновления не работает.";
				Window win = new Window ("");
				MessageDialog md = new MessageDialog (win, DialogFlags.DestroyWithParent,
					                   MessageType.Error, 
					                   ButtonsType.Ok,
					                   updMessage);
				md.Run ();
				md.Destroy ();
			}
		}

		protected static void ConfigFileUpdater ()
		{
			if (QSMain.Configsource.Configs ["Updater"] == null)
				QSMain.Configsource.AddConfig ("Updater");
			QSMain.Configsource.Configs ["Updater"].Set ("Check", (UpdaterDialog.updChecker ? "False" : "True"));
			QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
			QSMain.Configsource.Save ();
		}

		protected static void ProgressChanged (object sender, DownloadProgressChangedEventArgs e)
		{
			Application.Invoke (delegate {
				updPr.Fraction = e.ProgressPercentage / 100.0;
			});
		}

		protected static void Completed (object sender, AsyncCompletedEventArgs e)
		{
			logger.Info ("Скачивание обновления завершено.");
			if (updWin.IsMapped) {
				StartProcess ();
				Application.Quit ();
			}
		}

		protected static void StartProcess ()
		{
			logger.Info ("Запускаем файл установки обновления.");
			System.Diagnostics.Process File = new System.Diagnostics.Process ();
			File.StartInfo.FileName = tempPath + @"\QSInstaller.exe";
			updWin.Destroy ();
			File.Start ();
		}
	}
}

