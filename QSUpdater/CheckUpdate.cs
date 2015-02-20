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
		static string checkVersion, checkResult, serialNumber, tempPath = System.IO.Path.GetTempPath ();
		static Logger logger = LogManager.GetCurrentClassLogger ();
		static ProgressBar updateProgress;
		static UpdateResult updateResult;
		static Uri address = new Uri ("http://saas.qsolution.ru:2048/Updater");
		static Window updateWindow = new Window ("Подождите...");

		static public void StartCheckUpdateThread (bool showAnyway = false)
		{
			Thread loadThread = new Thread (() => ThreadWorks (showAnyway));
			loadThread.Start ();
		}

		static void ThreadWorks (bool showAnyway)
		{
			try {
				logger.Info ("Получаем данные от сервера");
				string parameters = String.Format ("product.{0};edition.{1};serial.{2};major.{3};minor.{4};build.{5};revision.{6}",
				                                   MainSupport.ProjectVerion.Product,
				                                   MainSupport.ProjectVerion.Edition,
				                                   serialNumber,
				                                   MainSupport.ProjectVerion.Version.Major, 
				                                   MainSupport.ProjectVerion.Version.Minor, 
				                                   MainSupport.ProjectVerion.Version.Build, 
				                                   MainSupport.ProjectVerion.Version.Revision); 
				IUpdateService service = new WebChannelFactory<IUpdateService> (new WebHttpBinding { AllowCookies = true }, address)
					.CreateChannel ();
				updateResult = service.checkForUpdate (parameters);
				Application.Invoke (delegate {
					if (QSMain.Configsource.Configs ["Updater"] != null) {
						checkVersion = QSMain.Configsource.Configs ["Updater"].Get ("NewVersion", String.Empty);
						checkResult = QSMain.Configsource.Configs ["Updater"].Get ("Check", String.Empty);
					}
					if (showAnyway || (updateResult.HasUpdate &&
					    (checkResult == "True" || checkResult == String.Empty || checkVersion != updateResult.NewVersion)))
						ShowDialog ();
				});
			} catch (Exception ex) {
				if (showAnyway)
					ShowErrorDialog ();
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
			}
		}

		static void ShowDialog ()
		{
			VBox vbox = new VBox ();
			WebClient webClient = new WebClient ();
			webClient.DownloadFileCompleted += delegate {
				if (updateWindow.IsMapped) {
					logger.Info ("Скачивание обновления завершено. Запускаем установку...");
					System.Diagnostics.Process File = new System.Diagnostics.Process ();
					File.StartInfo.FileName = tempPath + @"\QSInstaller.exe";
					updateWindow.Destroy ();
					File.Start ();
					Application.Quit ();
				}
			};
			webClient.DownloadProgressChanged += (sender, e) => Application.Invoke (delegate {
				updateProgress.Fraction = e.ProgressPercentage / 100.0;
			});

			try {
				UpdaterDialog updaterDialog;
				if (updateResult.HasUpdate)
					updaterDialog = new UpdaterDialog (String.Format ("<b>Доступна новая версия программы!</b>\n" +
					"Доступная версия: {0}. (У вас установлена версия {1})\n" +
					"Вы хотите скачать и установить новую версию?\n\n" +
					(updateResult.UpdateDescription != String.Empty ? "<b>Информация об обновлении:</b>\n{2}" : "{2}"), 
					                                                  updateResult.NewVersion, MainSupport.ProjectVerion.Version, updateResult.UpdateDescription), updateResult);
				else
					updaterDialog = new UpdaterDialog (String.Format ("<b>Ваша версия программного продукта: {0}.</b>\n\n" +
					"На данный момент это актуальная версия продукта.\n", MainSupport.ProjectVerion.Version), updateResult);
					                                                                       
				int result = updaterDialog.Run ();
				updaterDialog.Destroy ();

				if ((ResponseType)result == ResponseType.Ok) {
					logger.Info ("Скачивание обновления началось.");
					updateWindow.SetSizeRequest (300, 25); 
					updateWindow.Resizable = false;
					updateWindow.SetPosition (WindowPosition.Center);
					updateProgress = new ProgressBar ();
					updateWindow.Add (vbox); 
					updateProgress.Text = "Новая версия скачивается, подождите...";
					vbox.PackStart (updateProgress, true, true, 0);

					updateWindow.ShowAll ();
					webClient.DownloadFileAsync (new Uri (updateResult.FileLink), tempPath + @"\QSInstaller.exe");

				} else if (updateResult.HasUpdate)
					ConfigFileUpdater ((ResponseType)result == ResponseType.Cancel);
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
				ShowErrorDialog ();
			}
		}

		static void ShowErrorDialog ()
		{
			Window win = new Window ("Ошибка");
			MessageDialog md = new MessageDialog (win, DialogFlags.DestroyWithParent,
			                                      MessageType.Error, 
			                                      ButtonsType.Ok,
			                                      "Извините, сервер обновления не работает.");
			md.Run ();
			md.Destroy ();
		}

		static void ConfigFileUpdater (bool check)
		{
			if (QSMain.Configsource.Configs ["Updater"] == null)
				QSMain.Configsource.AddConfig ("Updater");
			QSMain.Configsource.Configs ["Updater"].Set ("Check", check);
			QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", updateResult.NewVersion);
			QSMain.Configsource.Save ();
		}
	}
}