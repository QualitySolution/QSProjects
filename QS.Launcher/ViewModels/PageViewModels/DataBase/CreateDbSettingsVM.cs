using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class CreateDbSettingsVM : DbOperationSettingsVM {
		public CreateDbSettingsVM(IDbProvider provider, Connection connection, IServiceProvider services)
			: base(provider, connection, services) {
			SetValidity(this.WhenAnyValue(x => x.DbName, x => x.DbTitle,
				(name, title) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title)));
		}

		public override string Title => "Создание базы данных";

		private string dbTitle;
		public string DbTitle {
			get => dbTitle;
			set => this.RaiseAndSetIfChanged(ref dbTitle, value);
		}

		private string dbName;
		public string DbName {
			get => dbName;
			set => this.RaiseAndSetIfChanged(ref dbName, value);
		}

		private string importDumpFilePath;
		public string ImportDumpFilePath {
			get => importDumpFilePath;
			set => this.RaiseAndSetIfChanged(ref importDumpFilePath, value);
		}

		public bool CanImportDump => Provider is MariaDBProvider
			|| Connection?.ConnectionType?.SupportsDatabaseCreation(Services) == true;

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			var phases = new List<DbCreationPhase> {
				new DbCreationPhase(
					"Создание базы данных",
					args => args.Provider.CreateDatabase(DbName, DbTitle, Services))
			};

			if(!string.IsNullOrWhiteSpace(ImportDumpFilePath) && Provider is MariaDBProvider) {
				phases.Add(new DbCreationPhase(
					"Импорт дампа в базу данных",
					args => {
						((MariaDBProvider)args.Provider).ImportDatabase(
							DbName, ImportDumpFilePath, args.Progress, args.CancellationToken, DbTitle);
						args.CancellationToken.ThrowIfCancellationRequested();
						return true;
					}));
			}
			else if(Connection.ConnectionType.SupportsDatabaseCreation(Services)) {
				phases.Add(new DbCreationPhase(
					"Наполнение базы данных",
					args => {
						args.ImportDumpFilePath = ImportDumpFilePath;
						IDbCreatorModel creator = Connection.ConnectionType.CreateCreator(args);
						return creator.RunCreation(DbName, DbTitle);
					}));
			}

			return phases;
		}
	}
}
