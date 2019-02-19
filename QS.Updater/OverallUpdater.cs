using System;
using QSSupportLib;

namespace QS.Updater
{
	public class OverallUpdater
	{
		private IUpdaterUI updaterUI;
		private readonly IApplicationInfo application;
		private readonly ISkipVersionState skip;
		private readonly IDBUpdater dBUpdater;

		public OverallUpdater(IUpdaterUI updaterUI, IApplicationInfo application, ISkipVersionState skips, IDBUpdater dBUpdater)
		{
			this.updaterUI = updaterUI;
			this.application = application;
			this.skip = skips;
			this.dBUpdater = dBUpdater;
		}

		public void RunCheckVersion(bool updateDB, bool updateApp)
		{
			//FIXME Убрать зависимотсь от QSSupport
			CheckBaseVersion.Check();

			if(CheckBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess && updateDB) {
				dBUpdater.CheckUpdateDB();
				RunCheckVersion(updateDB, updateApp);
				return;
			}

			if(CheckBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater && updateApp) {
				CreateAppUpdater().StartCheckUpdateThread(UpdaterFlags.UpdateRequired);
			}

			if(CheckBaseVersion.ResultFlags != CheckBaseResult.Ok) {
				updaterUI.InteractiveMessage.ShowMessage(Dialog.ImportanceLevel.Warning, CheckBaseVersion.TextMessage);
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
