using System;
using Autofac;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class VersionCheckerService
	{
		private readonly CheckBaseVersion checkBaseVersion;
		private readonly ILifetimeScope autofacScope;

		public VersionCheckerService(CheckBaseVersion checkBaseVersion, ILifetimeScope autofacScope)
		{
			this.checkBaseVersion = checkBaseVersion ?? throw new ArgumentNullException(nameof(checkBaseVersion));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
		}

		public void RunUpdate(bool updateDB = true, bool updateApp = true)
		{
			checkBaseVersion.Check();

			if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess && updateDB) {
				var dBUpdater = autofacScope.Resolve<IDBUpdater>();
				dBUpdater.CheckUpdateDB();
				RunUpdate(updateDB, updateApp);
				return;
			}

			if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater && updateApp) {
				var appUpdater = autofacScope.Resolve<ApplicationUpdater>();
				appUpdater.StartCheckUpdateThread(UpdaterFlags.UpdateRequired);
			}

			if(checkBaseVersion.ResultFlags != CheckBaseResult.Ok) {
				var interactive = autofacScope.Resolve<IInteractiveMessage>();
				interactive.ShowMessage(ImportanceLevel.Warning, checkBaseVersion.TextMessage);
				Environment.Exit(0);
			}

			if(updateApp) {
				var appUpdater = autofacScope.Resolve<ApplicationUpdater>();
				appUpdater.StartCheckUpdateThread(UpdaterFlags.StartInThread);
			}
		}
	}
}
