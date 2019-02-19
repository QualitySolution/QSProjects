using System;

namespace QS.Updater
{
	public static class MainUpdater
	{
		public static void RunCheckVersion(bool updateDB, bool updateApp, bool installMicroUpdate)
			{
			var app = new ApplicationVersionInfo();
			var skips = new SkipVersionStateIniConfig();
			var updater = new OverallUpdater(
				new UpdaterGtkUI(app, skips),
				app,
				skips,
				new SQLDBUpdater()
			);
			updater.RunCheckVersion(updateDB, updateApp);
		}

		public static void CheckAppVersionShowAnyway()
		{
			var app = new ApplicationVersionInfo();
			var skips = new SkipVersionStateIniConfig();
			var updater = new ApplicationUpdater(
				new UpdaterGtkUI(app, skips),
				UpdateService.GetService(),
				app, skips
			);
			updater.StartCheckUpdateThread(UpdaterFlags.ShowAnyway);
		}
	}
}

