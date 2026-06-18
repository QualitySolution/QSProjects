using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using QS.DbManagement;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class BackupDbSettingsVM : DbOperationSettingsVM {
		private readonly DbInfo database;

		public BackupDbSettingsVM(DbInfo database, IDbProvider provider, Connection connection, IServiceProvider services)
			: base(provider, connection, services) {
			this.database = database ?? throw new ArgumentNullException(nameof(database));

			BackupTargetTitle = string.IsNullOrEmpty(database.BaseName) ? database.Title : database.BaseName;
			BackupFilePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"Backups",
				string.Format("{0}-{1:yyMMdd-HHmm}.sql", BackupTargetTitle, DateTime.Now));

			SetValidity(this.WhenAnyValue(x => x.BackupFilePath, path => !string.IsNullOrWhiteSpace(path)));
		}

		public override string Title => "Создание резервной копии базы данных";

		public string BackupTargetTitle { get; }

		private string backupFilePath;
		public string BackupFilePath {
			get => backupFilePath;
			set => this.RaiseAndSetIfChanged(ref backupFilePath, value);
		}

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			return new[] {
				new DbCreationPhase(
					"Создание резервной копии базы данных",
					args => {
						args.Provider.BackupDatabase(database, BackupFilePath, args.Progress, args.CancellationToken);
						args.CancellationToken.ThrowIfCancellationRequested();
						return true;
					})
			};
		}
	}
}
