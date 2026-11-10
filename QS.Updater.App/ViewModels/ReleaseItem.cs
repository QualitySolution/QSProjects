using System;
using System.Diagnostics;
using System.Windows.Input;
using QS.Commands;
using QS.Dialog;
using QS.Updates;

namespace QS.Updater.App.ViewModels {
	public sealed class ReleaseItem {
		public ReleaseItem(ReleaseInfo release, IInteractiveMessage interactive) {
			Version = release.Version;
			DatabaseUpdate = release.DatabaseUpdate;
			NewsLink = release.NewsLink;
			Date = release.Date?.ToDateTime().ToLongDateString() ?? string.Empty;
			Changes = release.Changes?.Replace('*', '✦') ?? string.Empty;
			DatabaseUpdateText = release.DatabaseUpdate == DatabaseUpdate.BreakingChange
				? "Требуется обновление базы данных нарушающее обратную совместимость с предыдущими версиями."
				: release.DatabaseUpdate == DatabaseUpdate.Required ? "Требуется обновление базы данных." : string.Empty;
			HasDatabaseUpdate = !string.IsNullOrEmpty(DatabaseUpdateText);
			HasNews = !string.IsNullOrWhiteSpace(release.NewsLink);
			OpenNewsCommand = new DelegateCommand(() => {
				try { Process.Start(new ProcessStartInfo(release.NewsLink) { UseShellExecute = true }); }
				catch(Exception ex) {
					NLog.LogManager.GetCurrentClassLogger().Error(ex, "Не удалось открыть новость о выпуске.");
					interactive.ShowMessage(ImportanceLevel.Error, "Не удалось открыть новость о выпуске.");
				}
			}, () => HasNews);
		}
		public DatabaseUpdate DatabaseUpdate { get; }
		public string NewsLink { get; }
		public string Version { get; }
		public string Date { get; }
		public string Changes { get; }
		public string DatabaseUpdateText { get; }
		public bool HasDatabaseUpdate { get; }
		public bool IsBreakingDatabaseUpdate => DatabaseUpdate == DatabaseUpdate.BreakingChange;
		public bool HasNews { get; }
		public ICommand OpenNewsCommand { get; }
	}

}
