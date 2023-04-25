using System;
using System.Linq;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
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
		private readonly IInteractiveMessage interactive;
		private readonly IGuiDispatcher gui;
		private readonly ISkipVersionState skipVersionState;
		private readonly ParametersService parametersService;

		public ApplicationUpdater(
			ReleasesService releasesService,
			IApplicationInfo applicationInfo,
			INavigationManager navigation,
			IInteractiveMessage interactive,
			IGuiDispatcher gui,
			ISkipVersionState skipVersionState = null,
			ParametersService parametersService = null) {
			this.releasesService = releasesService ?? throw new ArgumentNullException(nameof(releasesService));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.gui = gui ?? throw new ArgumentNullException(nameof(gui));
			this.skipVersionState = skipVersionState;
			this.parametersService = parametersService;
		}

		public void CheckUpdate(bool manualRun) {
			logger.Info("Запрашиваем информацию о новых версиях с сервера");
			string serial = parametersService?.Dynamic.serial_number ?? String.Empty;
			ReleaseChannel.TryParse(parametersService?.Dynamic.UpdateChannel, out ReleaseChannel channel);
			ReleaseInfo[] releases = Array.Empty<ReleaseInfo>();
			try {
				 releases = releasesService.CheckForUpdates(
					applicationInfo.ProductCode,
					applicationInfo.Version.VersionToShortString(),
					applicationInfo.Modification ?? String.Empty,
					serial,
					channel
					);
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка доступа к серверу обновления.");
				if(manualRun)
					interactive.ShowMessage(ImportanceLevel.Error, "Не удалось подключиться к серверу обновлений.\nПожалуйста, повторите попытку позже.");
			}
			if(!releases.Any() && manualRun) {
				
				interactive.ShowMessage(ImportanceLevel.Info, $"<b>Ваша версия программного продукта: {applicationInfo.Version.VersionToShortString()}</b>\n" +
				                                              "На данный момент это самая последняя версия.");
				logger.Info("Нет новых версий программы.");
				return;
			}

			if(releases.Any()) {
				var updateToVersion = Version.Parse(releases.First().Version);
				if(manualRun || !skipVersionState.IsSkippedVersion(updateToVersion)) {
					var page = navigation.OpenViewModel<NewVersionViewModel, ReleaseInfo[]>(null, releases);
					var isClosed = false;
					CloseSource source = CloseSource.Self;
					page.PageClosed += (sender, e) => {
						isClosed = true;
						source = e.CloseSource;
					};
					gui.WaitInMainLoop(() => isClosed);
				}
			}
			logger.Info("Ок");
		}
	}
}
