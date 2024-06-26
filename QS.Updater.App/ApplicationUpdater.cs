using System;
using System.Linq;
using Gamma.Utilities;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Updater.App.ViewModels;
using QS.Updates;
using QS.Utilities.Text;

namespace QS.Updater.App {
	public class ApplicationUpdater : IAppUpdater
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private readonly ReleasesService releasesService;
		private readonly IApplicationInfo applicationInfo;
		private readonly INavigationManager navigation;
		private readonly IInteractiveService interactive;
		private readonly IGuiDispatcher gui;
		private readonly IApplicationQuitService quitService;
		private readonly ISkipVersionState skipVersionState;
		private readonly IUpdateChannelService channelService;
		private readonly ParametersService parametersService;

		public ApplicationUpdater(
			ReleasesService releasesService,
			IApplicationInfo applicationInfo,
			INavigationManager navigation,
			IInteractiveService interactive,
			IGuiDispatcher gui,
			IApplicationQuitService quitService,
			ISkipVersionState skipVersionState = null,
			IUpdateChannelService channelService = null,
			ParametersService parametersService = null) {
			this.releasesService = releasesService ?? throw new ArgumentNullException(nameof(releasesService));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.gui = gui ?? throw new ArgumentNullException(nameof(gui));
			this.quitService = quitService ?? throw new ArgumentNullException(nameof(quitService));
			this.skipVersionState = skipVersionState;
			this.channelService = channelService;
			this.parametersService = parametersService;
		}

		public UpdateInfo CheckUpdate(bool manualRun) {
			ReleaseChannel.TryParse(channelService?.CurrentChannel.ToString(), out ReleaseChannel channel);
			return CheckUpdate(manualRun, channel);
		}

		private UpdateInfo CheckUpdate(bool manualRun, ReleaseChannel channel) {
			logger.Info("Запрашиваем информацию о новых версиях с сервера");
			string serial = parametersService?.Dynamic.serial_number ?? String.Empty;
			CheckForUpdatesResponse response;
			try {
				 response = releasesService.CheckForUpdates(
					applicationInfo.ProductCode,
					applicationInfo.Version.VersionToShortString(),
					applicationInfo.Modification ?? String.Empty,
					serial,
					channel
					);
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка доступа к серверу обновления.");
				return new UpdateInfo("Ошибка доступа к серверу обновления", "Не удалось подключиться к серверу обновлений.\nПожалуйста, повторите попытку позже", UpdateStatus.Error, ImportanceLevel.Error);
			}

			if (!response.Releases.Any()) 
			{
				UpdateInfo updateInfo;
				if (!string.IsNullOrWhiteSpace(response.Title) && !string.IsNullOrWhiteSpace(response.Message)) 
				{
					logger.Info(response.Title);
					updateInfo = new UpdateInfo(response.Title, response.Message, UpdateStatus.ExternalError, ImportanceLevel.Warning);
				}
				else {

					logger.Info("Нет новых версий программы.");
					updateInfo = new UpdateInfo("Нет новых версий программы",
						$"<b>Ваша версия программного продукта: {applicationInfo.Version.VersionToShortString()}</b>\nНа данный момент это самая последняя версия",
						UpdateStatus.UpToDate, ImportanceLevel.Info);
				}

				if (manualRun) 
				{
					interactive.ShowMessage(updateInfo.ImportanceLevel, updateInfo.Message, updateInfo.Title);
				}

				return updateInfo;
			}
			
			var updateToVersion = Version.Parse(response.Releases.First().Version);
			if(manualRun || !skipVersionState.IsSkippedVersion(updateToVersion)) {
				var page = navigation.OpenViewModel<NewVersionViewModel, ReleaseInfo[]>(null, response.Releases.ToArray());
				var isClosed = false;
				string title = string.Empty;
				string message = string.Empty;
				UpdateStatus status = UpdateStatus.Error;
				page.PageClosed += (sender, e) => 
				{
					isClosed = true;
					if (e.CloseSource == CloseSource.Cancel) 
					{
						title = "Обновление пропущено";
						status = UpdateStatus.Skip;
					}
					else if (e.CloseSource == CloseSource.AppQuit) 
					{
						title = "Установка обновления";
						message = "Приложение будет закрыто";
						status = UpdateStatus.Ok;
						quitService.Quit();
					}
					else 
					{
						title = "Обновление отложено";
						status = UpdateStatus.Shelve;
					}
				};
				
				gui.WaitInMainLoop(() => isClosed);
				return new UpdateInfo(title, message, status, ImportanceLevel.Info);
			}
			
			logger.Info("Ок");
			return new UpdateInfo("Ок", string.Empty, UpdateStatus.Ok, ImportanceLevel.Info);
		}	

		public void TryAnotherChannel() {
			if(channelService == null || channelService.AvailableChannels.All(x => x == channelService.CurrentChannel))
				return;
			var cancelText = "Не надо";

			var buttons = channelService.AvailableChannels
				.Where(x => x != channelService.CurrentChannel)
				.Select(x => x.GetEnumTitle())
				.Union(new[] { cancelText }).ToArray();
			var answer = interactive.Question(buttons,
				"В используемом канале нет новых версий. При этом у вас более новая версия БД. Хотите проверить обновления в другом канале?");
			if(answer == cancelText || String.IsNullOrEmpty(answer))
				return;
			var channel = channelService.AvailableChannels.First(x => x.GetEnumTitle() == answer);
			ReleaseChannel.TryParse(channel.ToString(), out ReleaseChannel releaseChannel);
			_ = CheckUpdate(false, releaseChannel);
		}
	}
}
