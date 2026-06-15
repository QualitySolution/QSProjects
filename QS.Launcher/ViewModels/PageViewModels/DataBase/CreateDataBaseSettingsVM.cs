using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public enum DbWizardOperation {
		Create,
		Backup
	}

	/// <summary>
	/// Универсальная страница настроек операции с базой: ввод параметров создания базы
	/// (с необязательным импортом дампа для MariaDB) либо выбор файла для резервной копии.
	/// Конкретную операцию (состав пайплайна фаз) строит <see cref="GoToProgress"/>.
	/// </summary>
	public class CreateDataBaseSettingsVM : CarouselPageVM {
		public IDbProvider Provider { get; private set; }
		public Connection Connection { get; private set; }
		private readonly IServiceProvider services;

		private DbInfo backupTarget;

		private DbWizardOperation operation = DbWizardOperation.Create;
		public DbWizardOperation Operation {
			get => operation;
			private set {
				this.RaiseAndSetIfChanged(ref operation, value);
				this.RaisePropertyChanged(nameof(IsCreateMode));
				this.RaisePropertyChanged(nameof(IsBackupMode));
				this.RaisePropertyChanged(nameof(CanImportDump));
			}
		}

		public bool IsCreateMode => Operation == DbWizardOperation.Create;
		public bool IsBackupMode => Operation == DbWizardOperation.Backup;

		/// <summary>
		/// Импорт дампа при создании: MariaDB напрямую, облако - через свой creator
		/// (доступен, когда тип подключения поддерживает создание, т.е. есть creator).
		/// </summary>
		public bool CanImportDump => Operation == DbWizardOperation.Create
			&& (Provider is MariaDBProvider
				|| Connection?.ConnectionType?.SupportsDatabaseCreation(services) == true);

		#region Создание

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

		/// <summary>Необязательный путь к SQL-дампу, заливаемому в созданную базу (только MariaDB).</summary>
		private string importDumpFilePath;
		public string ImportDumpFilePath {
			get => importDumpFilePath;
			set => this.RaiseAndSetIfChanged(ref importDumpFilePath, value);
		}

		#endregion

		#region Резервная копия

		private string backupTargetTitle;
		public string BackupTargetTitle {
			get => backupTargetTitle;
			private set => this.RaiseAndSetIfChanged(ref backupTargetTitle, value);
		}

		private string backupFilePath;
		public string BackupFilePath {
			get => backupFilePath;
			set => this.RaiseAndSetIfChanged(ref backupFilePath, value);
		}

		#endregion

		public ReactiveCommand<Unit, Unit> ProceedCommand { get; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }

		/// <summary>
		/// Сообщает заинтересованным о том, что только что создана
		/// progress-VM и пора подписаться на её события
		/// </summary>
		public event Action<CreateDataBaseProgressVM> ProgressPageRequested;

		public CreateDataBaseSettingsVM(IServiceProvider services) {
			this.services = services ?? throw new ArgumentNullException(nameof(services));

			var canProceed = this.WhenAnyValue(
				x => x.Operation, x => x.DbName, x => x.DbTitle, x => x.BackupFilePath,
				(op, name, title, path) => op == DbWizardOperation.Create
					? !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title)
					: !string.IsNullOrWhiteSpace(path));

			ProceedCommand = ReactiveCommand.Create(GoToProgress, canProceed);
			CancelCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));
		}

		public void SetDbSettings(IDbProvider provider, Connection connection) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			ImportDumpFilePath = null;
			Operation = DbWizardOperation.Create;
		}

		public void SetBackupSettings(IDbProvider provider, Connection connection, DbInfo database) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			backupTarget = database ?? throw new ArgumentNullException(nameof(database));
			Operation = DbWizardOperation.Backup;

			BackupTargetTitle = database.Title;
			// У облачного DbInfo нет BaseName - подставляем Title в имя файла.
			var fileBaseName = string.IsNullOrEmpty(database.BaseName) ? database.Title : database.BaseName;
			BackupFilePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"Резервные копии",
				string.Format("{0}-{1:yyMMdd-HHmm}.sql", fileBaseName, DateTime.Now));
		}

		private void GoToProgress() {
			var progress = ActivatorUtilities.GetServiceOrCreateInstance<CreateDataBaseProgressVM>(services);

			DbCreationPhase[] pipeline;
			if(Operation == DbWizardOperation.Backup) {
				progress.OperationTitle = "Создание резервной копии базы данных";
				pipeline = new[] {
					new DbCreationPhase(
						"Создание резервной копии базы данных",
						args => {
							// BackupDatabase теперь на IDbProvider - работает и для MariaDB, и для облака.
							args.Provider.BackupDatabase(
								backupTarget, BackupFilePath, args.Progress, args.CancellationToken);
							args.CancellationToken.ThrowIfCancellationRequested();
							return true;
						})
				};
			}
			else {
				progress.OperationTitle = "Создание базы данных";
				var phases = new System.Collections.Generic.List<DbCreationPhase> {
					new DbCreationPhase(
						"Создание базы данных",
						args => args.Provider.CreateDatabase(DbName, DbTitle, services))
				};

				if(!string.IsNullOrWhiteSpace(ImportDumpFilePath) && Provider is MariaDBProvider) {
					// Залить выбранный дамп вместо стандартного наполнения скриптом.
					phases.Add(new DbCreationPhase(
						"Импорт дампа в базу данных",
						args => {
							((MariaDBProvider)args.Provider).ImportDatabase(
								DbName, ImportDumpFilePath, args.Progress, args.CancellationToken);
							args.CancellationToken.ThrowIfCancellationRequested();
							return true;
						}));
				}
				else if(Connection.ConnectionType.SupportsDatabaseCreation(services)) {
					phases.Add(new DbCreationPhase(
						"Наполнение базы данных",
						args => {
							// Передаём дамп в creator: облачный creator зальёт его вместо скрипта.
							args.ImportDumpFilePath = ImportDumpFilePath;
							IDbCreatorModel creator = Connection.ConnectionType.CreateCreator(args);
							return creator.RunCreation(DbName, DbTitle);
						}));
				}

				pipeline = phases.ToArray();
			}

			progress.SetPipeline(Provider, Connection, pipeline);
			ProgressPageRequested?.Invoke(progress);
			PushPageCommand?.Execute(progress);
		}
	}
}
