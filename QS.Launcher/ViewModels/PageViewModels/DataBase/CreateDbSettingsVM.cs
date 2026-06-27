using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Project.Versioning;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class CreateDbSettingsVM : DbOperationSettingsVM {
		public CreateDbSettingsVM(IDbProvider provider, Connection connection, IServiceProvider services)
			: base(provider, connection, services) {
			SetValidity(this.WhenAnyValue(x => x.DbName, x => x.DbTitle,
				(name, title) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title)));

			CanImportDump = services.GetRequiredService<DbCapabilities>().CanImport(provider);
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

		public bool CanImportDump;

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			return new[] {
				new DbCreationPhase("Создание базы данных", args => {
					IDbFillStrategy fillStrategy;
					if(string.IsNullOrWhiteSpace(ImportDumpFilePath))
						fillStrategy = args.ServiceProvider.GetRequiredService<IDbFillStrategy>();
					else
						fillStrategy = new DumpDbFillStrategy(ImportDumpFilePath);

					var request = new DbCreationRequest {
						DbName = DbName,
						DbTitle = DbTitle,
						FillStrategy = fillStrategy,
						ApplicationInfo = args.ServiceProvider.GetService<IApplicationInfo>(),
						Progress = args.Progress,
						Interaction = args.Interaction,
						CancellationToken = args.CancellationToken,
					};
					return args.Provider.CreateDatabase(request);
				})
			};
		}
	}
}
