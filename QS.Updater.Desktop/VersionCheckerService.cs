using System;
using QS.Dialog;
using QS.Project.Services;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class VersionCheckerService
	{
		private readonly CheckBaseVersion checkBaseVersion;
		private readonly IAppUpdater applicationUpdater;
		private readonly IDBUpdater dbUpdater;

		public VersionCheckerService(CheckBaseVersion checkBaseVersion, IInteractiveMessage interactive, IApplicationQuitService quitService, IAppUpdater applicationUpdater = null, IDBUpdater dbUpdater = null) {
			this.checkBaseVersion = checkBaseVersion ?? throw new ArgumentNullException(nameof(checkBaseVersion));
			this.applicationUpdater = applicationUpdater;
			this.dbUpdater = dbUpdater;
		}

		public UpdateInfo? RunUpdate(bool isOffAutoUpdateChannelActive) 
		{
			UpdateInfo? updateInfo = null;
			if (applicationUpdater != null && !isOffAutoUpdateChannelActive) {
				updateInfo = applicationUpdater.CheckUpdate(false);
				if(updateInfo?.Status == UpdateStatus.AppUpdateIsRunning) {
					return updateInfo;
				}
			}

			checkBaseVersion.Check();
			if(applicationUpdater != null && checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater) {
				applicationUpdater.TryAnotherChannel();
				checkBaseVersion.Check();
			}
			
			if(dbUpdater != null && (checkBaseVersion.ResultFlags == CheckBaseResult.Ok || checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess)) {
				if(dbUpdater.HasUpdates) {
					dbUpdater.UpdateDB();
					checkBaseVersion.Check();
				}
			}

			if (checkBaseVersion.ResultFlags != CheckBaseResult.Ok) {
				updateInfo = new UpdateInfo("", checkBaseVersion.TextMessage, UpdateStatus.BaseError, ImportanceLevel.Warning);
			}

			return updateInfo;
		}
	}
}
