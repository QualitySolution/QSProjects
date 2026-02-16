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
		private readonly IUpdateChannelService channelService;
		private readonly ParametersService parametersService;

		public ApplicationUpdater(
			ReleasesService releasesService,
			IApplicationInfo applicationInfo,
			INavigationManager navigation,
			IInteractiveService interactive,
			IGuiDispatcher gui,
			IApplicationQuitService quitService,
			IUpdateChannelService channelService = null,
			ParametersService parametersService = null) {
			this.releasesService = releasesService ?? throw new ArgumentNullException(nameof(releasesService));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.gui = gui ?? throw new ArgumentNullException(nameof(gui));
			this.quitService = quitService ?? throw new ArgumentNullException(nameof(quitService));
			this.channelService = channelService;
			this.parametersService = parametersService;
		}

		#region Результаты проверки
		public CheckForUpdatesResponse LastResponse { get; private set; }
		public UpdateInfo LastResults { get; private set; }
		public bool CanUpdate => LastResponse?.Releases.Any() ?? false;
		public Version UpdateToVersion => Version.Parse(LastResponse.Releases.First().Version);
		#endregion

		public UpdateInfo CheckUpdate() {
			// Если установлен канал OffAutoUpdate - автообновление отключено
			if(channelService?.CurrentChannel == UpdateChannel.OffAutoUpdate) {
				logger.Info("Автоматическое обновление отключено (канал OffAutoUpdate)");
				return new UpdateInfo("Автообновление отключено", "Автоматическое обновление отключено в настройках", UpdateStatus.Ok, ImportanceLevel.Info);
			}
			
			ReleaseChannel.TryParse(channelService?.CurrentChannel.ToString(), out ReleaseChannel channel);
			return CheckUpdate(channel);
		}

		private UpdateInfo CheckUpdate(ReleaseChannel channel) {
			logger.Info("Запрашиваем информацию о новых версиях с сервера");
			string serial = parametersService?.Dynamic.serial_number ?? String.Empty;
			try {
				LastResponse = releasesService.CheckForUpdates(
					applicationInfo.ProductCode,
					applicationInfo.Version.VersionToShortString(),
					applicationInfo.Modification ?? String.Empty,
					serial,
					channel
				);
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка доступа к серверу обновления.");
				return LastResults = new UpdateInfo("Ошибка доступа к серверу обновления",
					"Не удалось подключиться к серверу обновлений.\nПожалуйста, повторите попытку позже", UpdateStatus.ConnectionError,
					ImportanceLevel.Error);
			}

			if(LastResponse.Releases.Any()) {
				LastResults = new UpdateInfo("Ок", string.Empty, UpdateStatus.Ok, ImportanceLevel.Info);
			}
			else {
				if(!string.IsNullOrWhiteSpace(LastResponse.Title) && !string.IsNullOrWhiteSpace(LastResponse.Message)) {
					logger.Info(LastResponse.Title);
					LastResults = new UpdateInfo(LastResponse.Title, LastResponse.Message, UpdateStatus.ExternalError,
						ImportanceLevel.Warning);
				}
				else {
					logger.Info("Нет новых версий программы.");
					LastResults = new UpdateInfo("Нет новых версий программы",
						$"<b>Ваша версия программного продукта: {applicationInfo.Version.VersionToShortString()}</b>\nНа данный момент это самая последняя версия",
						UpdateStatus.UpToDate, ImportanceLevel.Info);
				}
			}
			return LastResults;
		}

		/// <summary>
		/// Показывает окно с информацией о новой версии и возможностью ее установить.
		/// </summary>
		public UpdateInfo? RunUpdate() {
			if(!CanUpdate) {
				interactive.ShowMessage(LastResults.ImportanceLevel, LastResults.Message, LastResults.Title);
				return null;
			}
			else {
				var page = navigation.OpenViewModel<NewVersionViewModel, ReleaseInfo[]>(null, LastResponse.Releases.ToArray());
				var isClosed = false;
				string title = string.Empty;
				string message = string.Empty;
				UpdateStatus status = UpdateStatus.Shelve;
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
						status = UpdateStatus.AppUpdateIsRunning;
						quitService.Quit();
					}
					else {
						title = "Обновление отложено";
						status = UpdateStatus.Shelve;
					}
				};
				
				gui.WaitInMainLoop(() => isClosed);
				return new UpdateInfo(title, message, status, ImportanceLevel.Info);
			}
		}	

		public UpdateInfo? TryAnotherChannel() {
			if(channelService == null || channelService.AvailableChannels.All(x => x == channelService.CurrentChannel))
				return null;
			var cancelText = "Не надо";

			var buttons = channelService.AvailableChannels
				.Where(x => x != channelService.CurrentChannel)
				.Select(x => x.GetEnumTitle())
				.Union(new[] { cancelText }).ToArray();
			var answer = interactive.Question(buttons,
				"В используемом канале нет новых версий. При этом у вас более новая версия БД. Хотите проверить обновления в другом канале?");
			if(answer == cancelText || String.IsNullOrEmpty(answer))
				return null;
			var channel = channelService.AvailableChannels.First(x => x.GetEnumTitle() == answer);
			ReleaseChannel.TryParse(channel.ToString(), out ReleaseChannel releaseChannel);
			return CheckUpdate(releaseChannel);
		}
	}
}
