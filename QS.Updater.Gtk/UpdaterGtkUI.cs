using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Project.VersionControl;
using QS.Utilities.Text;
using QSSupportLib;
using QSUpdater;

namespace QS.Updater
{
	public class UpdaterGtkUI : IUpdaterUI
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IApplicationInfo application;
		private readonly ISkipVersionState skip;

		public UpdaterGtkUI(IApplicationInfo application, ISkipVersionState skips) {
			this.application = application;
			this.skip = skips;
		}

		public IInteractiveMessage InteractiveMessage => new GtkMessageDialogsInteractive();

		public void ShowAppNewVersionDialog(UpdateResult result, UpdaterFlags flags)
		{
			if (flags.HasFlag(UpdaterFlags.StartInThread))
			{
				Application.Invoke(delegate
				{
					ShowDialog(result, flags);
				});
			}
			else
			{
				ShowDialog(result, flags);
			}
		}

		private void ShowDialog(UpdateResult result, UpdaterFlags flags)
		{
			string message = String.Empty;
			if (result.HasUpdate && !flags.HasFlag(UpdaterFlags.UpdateRequired))
				message = String.Format("<b>Доступна новая версия программы!</b>\n" +
				"Доступная версия: {0} (У вас установлена версия {1})\n" +
				"Вы хотите скачать и установить новую версию?\n\n" +
				(result.UpdateDescription != String.Empty ? "<b>Информация об обновлении:</b>\n{2}" : "{2}"),
										 result.NewVersion.VersionToShortString(),
										 application.Version.VersionToShortString(),
										 result.UpdateDescription);
			else if (result.HasUpdate && flags.HasFlag(UpdaterFlags.UpdateRequired))
				message = String.Format("<b>Доступна новая версия программы!</b>\n" +
				"Доступная версия: {0} (У вас установлена версия {1})\n" +
				"<b>Для продолжения работы вам необходимо установить данное обновление.</b>\n\n" +
				(result.UpdateDescription != String.Empty ? "<b>Информация об обновлении:</b>\n{2}" : "{2}"),
										 result.NewVersion.VersionToShortString(),
										 application.Version.VersionToShortString(),
										 result.UpdateDescription);
			else if (!result.HasUpdate && !flags.HasFlag(UpdaterFlags.UpdateRequired))
				message = String.Format("<b>Ваша версия программного продукта: {0}</b>\n\n" +
				"На данный момент это самая последняя версия.\n" +
										 "Обновление не требуется.",
										 application.Version.VersionToShortString());
			else if (!result.HasUpdate && flags.HasFlag(UpdaterFlags.UpdateRequired))
			{
				InteractiveMessage.ShowMessage(ImportanceLevel.Error, "Необходимое обновление программы не найдено.\n" + CheckBaseVersion.TextMessage);
				Environment.Exit(1);
			}

			NewVersionDialog updaterDialog = new NewVersionDialog(message, result, flags);
			ResponseType resultOfDialog = (ResponseType)updaterDialog.Run();
			updaterDialog.Destroy();

			if (resultOfDialog == ResponseType.Ok)
			{
				Window updateWindow = new Window("Подождите...");
				string tempPath = Path.Combine(Path.GetTempPath(),
					  String.Format(@"QSInstaller-{0}.exe", Guid.NewGuid().ToString().Substring(0, 8)));

				bool loadingComplete = false;
				ProgressBar updateProgress;
				updateProgress = new ProgressBar();
				updateProgress.Text = "Новая версия скачивается, подождите...";
				VBox vbox = new VBox();
				vbox.PackStart(updateProgress, true, true, 0);
				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += (sender, e) => Application.Invoke(delegate {
					loadingComplete = true;
					var isMapped = updateWindow.IsMapped;
					updateWindow.Destroy();
					if (isMapped && e.Error == null && !e.Cancelled)
					{
						logger.Info("Скачивание обновления завершено. Запускаем установку...");
						Process File = new Process();
						File.StartInfo.FileName = tempPath;
						try
						{
							File.Start();
							Environment.Exit(0);
						}
						catch (Exception ex)
						{
							logger.Error(ex, "Не удалось запустить скачанный установщик.");
							InteractiveMessage.ShowMessage(ImportanceLevel.Error, "Не удалось запустить скачанный файл.");
						}
					}
					else if (e.Error != null)
					{
						logger.Error(e.Error, "Не удалось скачать файл обновления.");
						InteractiveMessage.ShowMessage(ImportanceLevel.Error, "Не удалось скачать файл.");
					}

				});
				webClient.DownloadProgressChanged += (sender, e) => Application.Invoke(delegate {
					updateProgress.Fraction = e.ProgressPercentage / 100.0;
				});
				updateWindow.SetSizeRequest (300, 25); 
				updateWindow.Resizable = false;
				updateWindow.SetPosition(WindowPosition.Center);
				if (flags.HasFlag(UpdaterFlags.UpdateRequired))
					updateWindow.DeleteEvent += delegate {
						Environment.Exit(0);
					};
				updateWindow.Add(vbox);

				updateWindow.ShowAll();
				logger.Info("Скачивание обновления началось.");
				logger.Debug("Скачиваем из {0} в {1}", result.FileLink, tempPath);

				webClient.DownloadFileAsync(new Uri(result.FileLink), tempPath);
				// Ждем окончания загрузки файла не возвращая управление, иначе в процессе скачивания продолжется работа, а это не надо во всех случаях
				while (!loadingComplete)
				{
					if (Gtk.Application.EventsPending())
						Gtk.Application.RunIteration();
					else
						Thread.Sleep(50);
				}
			}
			else if (flags.HasFlag(UpdaterFlags.UpdateRequired))
			{
				Environment.Exit(0);
			}
			else if (result.HasUpdate && resultOfDialog == ResponseType.No)
				skip.SaveSkipVersion(result.NewVersion);
		}
	}
}
