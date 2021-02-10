using System;
using Autofac;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class VersionCheckerService
	{
		private readonly IContainer container;

		public VersionCheckerService(IContainer container)
		{
			this.container = container ?? throw new ArgumentNullException(nameof(container));
		}

		public void RunUpdate(bool updateDB = true, bool updateApp = true)
		{
			using(var autofacScope = container.BeginLifetimeScope()) {
				var checkBaseVersion = autofacScope.Resolve<CheckBaseVersion>();
				checkBaseVersion.Check();

				if(updateDB) {
					var dBUpdater = autofacScope.Resolve<IDBUpdater>();
					if(dBUpdater.HasUpdates) {
						dBUpdater.UpdateDB();
						return;
					}
				}

				if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater && updateApp) {
					var appUpdater = autofacScope.Resolve<ApplicationUpdater>();
					appUpdater.StartCheckUpdate(UpdaterFlags.UpdateRequired, autofacScope);
				}

				if(checkBaseVersion.ResultFlags != CheckBaseResult.Ok) {
					var interactive = autofacScope.Resolve<IInteractiveMessage>();
					interactive.ShowMessage(ImportanceLevel.Warning, checkBaseVersion.TextMessage);
					Environment.Exit(0);
				}

				if(updateApp) {
					var appUpdater = autofacScope.Resolve<ApplicationUpdater>();
					appUpdater.StartCheckUpdateThread(UpdaterFlags.StartInThread, container);
				}
			}
		}
	}
}
