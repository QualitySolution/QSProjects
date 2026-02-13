using System;

namespace QS.Updater {
	public interface IAppUpdater {
		UpdateInfo CheckUpdate();
		UpdateInfo? RunUpdate();
		UpdateInfo? TryAnotherChannel();
		bool CanUpdate { get; }
		Version UpdateToVersion { get; }

	}
}
