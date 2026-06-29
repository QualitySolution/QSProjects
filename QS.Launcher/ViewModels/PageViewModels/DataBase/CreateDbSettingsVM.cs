using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Creation;
using QS.DbManagement.Entities;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.Generic;

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

		public override IEnumerable<DbCreationPhase> BuildPipeline() {
			return new[] {
				new DbCreationPhase("Создание базы данных", args => {
					var factory = args.ServiceProvider.GetRequiredService<DbCreationFactory>();

					var request = new DbCreationRequest {
						DbName = DbName,
						DbTitle = DbTitle,
						CreationFactory = factory,
						ApplicationInfo = args.ServiceProvider.GetService<IApplicationInfo>(),
						Interaction = args.ServiceProvider.GetRequiredService<IDbCreatorInteraction>(),
						// строку подключения заполнит провайдер
						CreationResources = new MySqlCreationResources {
							Progress = args.Progress,
							Interactions = args.ServiceProvider.GetRequiredService<IDbCreatorInteraction>(),
							Script = args.ServiceProvider.GetRequiredService<IDbScriptsConfiguration>().MakeCreationScript(),
							CancellationToken = args.CancellationToken }
					};
					return args.Provider.CreateDatabase(request);
				})
			};
		}
	}
}
