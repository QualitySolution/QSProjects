using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class ImportDbSettingsVM : DbOperationSettingsVM {
		public ImportDbSettingsVM(IDbManager provider, Connection connection, IServiceProvider services)
			: base(provider, connection, services) {
			SetValidity(this.WhenAnyValue(x => x.DbName, x => x.DbTitle, x => x.ImportDumpFilePath,
				(name, title, dump) => !string.IsNullOrWhiteSpace(name)
					&& !string.IsNullOrWhiteSpace(title)
					&& !string.IsNullOrWhiteSpace(dump)));
		}

		public override string Title => "Импорт базы данных из дампа";

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

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			return new[] {
				new DbCreationPhase("Импорт базы данных из дампа", args => {
					var request = new DbImportRequest {
						DbName = DbName,
						DbTitle = DbTitle,
						DumpFilePath = ImportDumpFilePath,
						DumpService = args.ServiceProvider.GetRequiredService<IDbDumpService>(),
						Progress = args.Progress,
						Interaction = args.ServiceProvider.GetRequiredService<IDbCreatorInteraction>(),
						ApplicationInfo = args.ServiceProvider.GetService<IApplicationInfo>(),
						CancellationToken = args.CancellationToken
					};
					return args.Provider.ImportDatabase(request);
				})
			};
		}
	}
}
