using System;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class VersionCheckerService
	{
		private readonly CheckBaseVersion checkBaseVersion;
		private readonly IInteractiveMessage interactive;
		private readonly IAppUpdater applicationUpdater;
		private readonly IDBUpdater dbUpdater;

		public VersionCheckerService(CheckBaseVersion checkBaseVersion, IInteractiveMessage interactive, IAppUpdater applicationUpdater = null, IDBUpdater dbUpdater = null) {
			this.checkBaseVersion = checkBaseVersion ?? throw new ArgumentNullException(nameof(checkBaseVersion));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.applicationUpdater = applicationUpdater;
			this.dbUpdater = dbUpdater;
		}

		public void RunUpdate()
		{
			if(applicationUpdater != null) {
				applicationUpdater.CheckUpdate(false);
			}
			
			checkBaseVersion.Check();
			if(dbUpdater != null && (checkBaseVersion.ResultFlags == CheckBaseResult.Ok || checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess)) {
				if(dbUpdater.HasUpdates) {
					dbUpdater.UpdateDB();
					checkBaseVersion.Check();
				}
			}

			if(checkBaseVersion.ResultFlags != CheckBaseResult.Ok) {
				interactive.ShowMessage(ImportanceLevel.Warning, checkBaseVersion.TextMessage);
				Environment.Exit(0);
			}
		}
	}
}
