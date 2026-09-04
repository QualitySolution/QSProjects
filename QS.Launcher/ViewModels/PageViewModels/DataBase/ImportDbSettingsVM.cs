using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Creation;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class ImportDbSettingsVM : DbOperationSettingsVM {
		public ImportDbSettingsVM(LauncherNavigation navigation, IDbManager provider, Connection connection,
			IServiceProvider services, IInteractiveMessage interactiveMessage)
			: base(navigation, provider, connection, services, interactiveMessage) {
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

		protected override string ValidationError() => SqlDumpFileValidator.Validate(ImportDumpFilePath);

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			// Наполнение из дампа.
			return new[] {
				new DbCreationPhase("Импорт базы данных из дампа", args => {
					var factory = args.ServiceProvider.GetRequiredService<DbCreationFactory>();

					var request = new DbCreationRequest {
						DbName = DbName,
						DbTitle = DbTitle,
						CreationFactory = factory,
						Interaction = args.ServiceProvider.GetRequiredService<IDbCreatorInteraction>(),
						// строку подключения заполнит провайдер
						CreationResources = new DbDumpResources {
							Progress = args.Progress,
							Interactions = args.ServiceProvider.GetRequiredService<IDbCreatorInteraction>(),
							DumpFilePath = ImportDumpFilePath,
							CancellationToken = args.CancellationToken }
					};
					return args.Provider.CreateDatabase(request);
				})
			};
		}
	}
}
