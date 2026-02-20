using System;

namespace QS.Updater {
	public interface IAppUpdater {
		UpdateInfo CheckUpdate();
		UpdateInfo RunUpdate();
		UpdateInfo? TryAnotherChannel();
		bool CanUpdate { get; }
		Version UpdateToVersion { get; }
		/// <summary>
		/// Определяет, имеет ли смысл предлагать пользователю переключение на другой канал обновлений.
		/// </summary>
		bool CanSwitchChannel { get; }
	}
}
