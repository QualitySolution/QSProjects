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
	public class CheckUpdate
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

		static public void StartCheckUpdateThread ()
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
				string parameters = String.Format ("product.{0};edition.{1};serial.{2};major.{3};minor.{4};build.{5};revision.{6}",
				                                   MainSupport.ProjectVerion.Product,
				                                   MainSupport.ProjectVerion.Edition,
				                                   serialNumber,
				                                   MainSupport.ProjectVerion.Version.Major, 
				                                   MainSupport.ProjectVerion.Version.Minor, 
				                                   MainSupport.ProjectVerion.Version.Build, 
				                                   MainSupport.ProjectVerion.Version.Revision); 
				res = svc.checkForUpdate (parameters);

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
				});
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
			}
		}

		public static void ShowDialog ()
		{
			UpdaterDialog updDlg;
			try {
				if (res.HasUpdate)
					updDlg = new UpdaterDialog (String.Format ("<b>Доступна новая версия программы!</b>\n" +
					"Доступная версия: {0}. (У вас установлена версия {1})\n" +
					"Вы хотите скачать и установить новую версию?\n\n" +
					(res.UpdateDescription != String.Empty ? "Информация об обновлении:\n{2}\n\n" : "{2}"), 
					                                           res.NewVersion, 
					                                           MainSupport.ProjectVerion.Version,
					                                           res.UpdateDescription));
				else
					updDlg = new UpdaterDialog (String.Format ("<b>Ваша версия программного продукта: {0}.</b>\n\n" +
					"На данный момент это актуальная версия продукта.\n", MainSupport.ProjectVerion.Version));
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

				} else if (res.HasUpdate)
					ConfigFileUpdater ((ResponseType)result == ResponseType.Cancel);
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

		protected static void ConfigFileUpdater (bool check)
		{
			if (QSMain.Configsource.Configs ["Updater"] == null)
				QSMain.Configsource.AddConfig ("Updater");
			QSMain.Configsource.Configs ["Updater"].Set ("Check", check);
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

