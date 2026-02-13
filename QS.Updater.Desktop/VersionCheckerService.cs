using System;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class VersionCheckerService
	{
		private readonly CheckBaseVersion checkBaseVersion;
		private readonly IAppUpdater applicationUpdater;
		private readonly IDBUpdater dbUpdater;
		private readonly ISkipVersionState skipVersionState;

		public VersionCheckerService(
			CheckBaseVersion checkBaseVersion,
			IAppUpdater applicationUpdater = null,
			IDBUpdater dbUpdater = null,
			ISkipVersionState skipVersionState = null) {
			this.checkBaseVersion = checkBaseVersion ?? throw new ArgumentNullException(nameof(checkBaseVersion));
			this.applicationUpdater = applicationUpdater;
			this.dbUpdater = dbUpdater;
			this.skipVersionState = skipVersionState;
		}

		public UpdateInfo? RunUpdate() 
		{
			UpdateInfo? updateInfo = null;
			checkBaseVersion.Check();
			if(applicationUpdater != null) {
				updateInfo = applicationUpdater.CheckUpdate();
				if(applicationUpdater.CanUpdate) {
					if(skipVersionState.IsSkippedVersion(applicationUpdater.UpdateToVersion) && checkBaseVersion.ResultFlags != CheckBaseResult.BaseVersionGreater) {
						updateInfo = new UpdateInfo("", "Пользователь пропустил эту версию приложения", UpdateStatus.Skip, ImportanceLevel.Warning);
					}
					else {
						updateInfo = applicationUpdater.RunUpdate();
					}
				}
				else if(checkBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater) {
					updateInfo = applicationUpdater.TryAnotherChannel();
					if(applicationUpdater.CanUpdate)
						updateInfo = applicationUpdater.RunUpdate();
				}
				
				if(updateInfo?.Status == UpdateStatus.AppUpdateIsRunning) {
					return updateInfo;
				}
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
