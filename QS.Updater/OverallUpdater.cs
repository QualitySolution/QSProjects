using System;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class OverallUpdater
	{
		private IUpdaterUI updaterUI;
		private readonly IApplicationInfo application;
		private readonly ISkipVersionState skip;
		private readonly IDBUpdater dBUpdater;
		private readonly CheckBaseVersion checkBaseVersion;

		public OverallUpdater(IUpdaterUI updaterUI, IApplicationInfo application, ISkipVersionState skips, IDBUpdater dBUpdater, CheckBaseVersion checkBaseVersion)
		{
			this.updaterUI = updaterUI;
			this.application = application;
			this.skip = skips;
			this.dBUpdater = dBUpdater;
			this.checkBaseVersion = checkBaseVersion;
		}

		public void RunCheckVersion(bool updateDB, bool updateApp)
		{
			checkBaseVersion.Check();

			if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess && updateDB) {
				dBUpdater.CheckUpdateDB();
				RunCheckVersion(updateDB, updateApp);
				return;
			}

			if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater && updateApp) {
				CreateAppUpdater().StartCheckUpdateThread(UpdaterFlags.UpdateRequired);
			}

			if(checkBaseVersion.ResultFlags != CheckBaseResult.Ok) {
				updaterUI.InteractiveMessage.ShowMessage(Dialog.ImportanceLevel.Warning, checkBaseVersion.TextMessage);
				Environment.Exit(0);
			}

			if (updateDB)
				dBUpdater.CheckMicroUpdatesDB();

			if(updateApp) {
				CreateAppUpdater().StartCheckUpdateThread(UpdaterFlags.StartInThread);
			}
		}

		private ApplicationUpdater CreateAppUpdater()
		{
			return new ApplicationUpdater(
				updaterUI,
				UpdateService.GetService(),
				application,
				skip);
		}
	}
}
