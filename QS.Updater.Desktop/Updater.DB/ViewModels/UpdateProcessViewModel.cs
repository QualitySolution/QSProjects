using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MySql.Data.MySqlClient;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Versioning;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Updater.DB.ViewModels
{
	public class UpdateProcessViewModel : WindowDialogViewModelBase, IOnCloseActionViewModel
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public bool Success { get; private set; }

		private readonly UpdateConfiguration configuration;
		IMySQLProvider SQLProvider;
		private readonly dynamic parametersService;
		private readonly IInteractiveService interactive;
		private readonly IGuiDispatcher guiDispatcher;
		CancellationTokenSource cancellation = new CancellationTokenSource();

		private UpdateHop[] hops;
		Version dbVersion;

		public UpdateProcessViewModel(
			INavigationManager navigation,
			UpdateConfiguration configuration,
			IMySQLProvider mySQLProvider,
			IApplicationInfo applicationInfo,
			ParametersService parametersService,
			IInteractiveService interactive,
			IGuiDispatcher guiDispatcher) : base(navigation)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			SQLProvider = mySQLProvider;
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			FileName = System.IO.Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"Резервные копии",
				String.Format("{0}{1:yyMMdd-HHmm}.sql", applicationInfo.ProductName, DateTime.Now)
			);

			IsModal = true;

			//FIXME Здесь проверка micro_updates оставлена для совместимости и возможности корректного обновления со старых версий программ. Удаление сделает невозможным начать обновление с установленного микроапдейта.
			dbVersion = this.parametersService.micro_updates != null ? this.parametersService.micro_updates(typeof(Version)) : this.parametersService.version(typeof(Version));
			hops = configuration.GetHopsToLast(dbVersion).ToArray();
			var targetVersion = hops.Last().Destination;
			Title = String.Format("Обновление: {0} → {1}",
						dbVersion.VersionToShortString(),
						targetVersion.VersionToShortString());
			if(hops.All(x => x.Destination.Major == dbVersion.Major && x.Destination.Minor == dbVersion.Minor))
				Description = $"Будут выполнены незначительные изменения в базе. Все версии программы {targetVersion.Major}.{targetVersion.Minor}.x смогут продолжить работать с базой без обновления.";
			else
				Description = $"После обновления базы, версии программы ниже {targetVersion.Major}.{targetVersion.Minor} не будут работать. Во избежании порчи данных, убедитесь что в момент обновления никто не использует базу в работе.";
		}

		#region Свойства для View

		private string fileName;
		public virtual string FileName {
			get => fileName;
			set => SetField(ref fileName, value);
		}

		private bool needCreateBackup = true;
		public virtual bool NeedCreateBackup {
			get => needCreateBackup;
			set => SetField(ref needCreateBackup, value);
		}

		private string commandsLog;

		public virtual string CommandsLog {
			get => commandsLog;
			set => SetField(ref commandsLog, value);
		}

		public IProgressBarDisplayable OperationProgress { get; set; }
		public IProgressBarDisplayable TotalProgress { get; set; }

		private bool buttonExcuteSensitive = true;
		public virtual bool ButtonExcuteSensitive {
			get => buttonExcuteSensitive;
			set => SetField(ref buttonExcuteSensitive, value);
		}

		public string Description { get; set; }

		#endregion
		#region Действия View

		public void Execute()
		{
			ButtonExcuteSensitive = false;
			try {
				if (NeedCreateBackup) {
					if (ExecuteBackup() || cancellation.IsCancellationRequested) {
						buttonExcuteSensitive = true;
						cancellation = new CancellationTokenSource();
						return;
					}
				}

				TotalProgress.Start(maxValue: hops.Length, text: String.Format("Обновление: {0} → {1}",
						dbVersion.VersionToShortString(),
						hops.Last().Destination.VersionToShortString()
					));
				foreach (var hop in hops) {
					if(cancellation.IsCancellationRequested)
						return;
					RunOneUpdate(hop);
					TotalProgress.Add();
				}

				TotalProgress.Close();
				logger.Info("Обновление до версии завершено.");
				Success = true;
				Close(false, CloseSource.Save);
			}
			catch (MySqlException ex) when (ex.Number == 1142) {
				logger.Error(ex, "Нет прав на доступ к таблицам базы данных, в момент выполнения обновления.");
				ButtonExcuteSensitive = false;
				interactive.ShowMessage(ImportanceLevel.Error, "У вас нет прав на выполение команд обновления базы на уровне MySQL\\MariaDB сервера. Получите права на изменение структуры таблиц базы данных или выполните обновление от пользователя root.");
			}
		}

		public void Cancel()
		{
			if(interactive.Question("Остановка процеса обновления на середине приведет к неработоспособному состоянию базы данны. Действительно хотите остановить?", "Остановка операции"))
				Close(false, CloseSource.Cancel);
		}

		#endregion

		#region Внутреннее
		#region Резервное копирование
		bool ExecuteBackup()
		{
			logger.Info("Создаем резервную копию базы.");
			if (String.IsNullOrWhiteSpace(FileName)) {
				interactive.ShowMessage(ImportanceLevel.Error, "Имя файла резервной копии пустое.");
				logger.Warn("Имя файла резервной копии пустое. Отмена.");
				return true;
			}

			var bwExport = new BackgroundWorker();
			bwExport.DoWork += BwExport_DoWork;
			bwExport.RunWorkerAsync();

			guiDispatcher.WaitInMainLoop(() => !bwExport.IsBusy, 50);
			OperationProgress.Close();
			return false;
		}

		void BwExport_DoWork(object sender, DoWorkEventArgs e)
		{
			using(MySqlCommand cmd = SQLProvider.DbConnection.CreateCommand()) {
				using(MySqlBackup mb = new MySqlBackup(cmd)) {
					var dir = System.IO.Path.GetDirectoryName(FileName);

					if(!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					logger.Debug(FileName);
					currentTable = null;
					mb.ExportProgressChanged += Mb_ExportProgressChanged;
					mb.ExportToFile(FileName);
				}
			}
		}

		private string currentTable;
		void Mb_ExportProgressChanged(object sender, ExportProgressArgs e)
		{
			if(cancellation.IsCancellationRequested)
				(sender as MySqlBackup).StopAllProcess();

			guiDispatcher.RunInGuiTread(delegate {
				if(currentTable == null)
					TotalProgress.Start(maxValue: e.TotalRowsInAllTables, text: "Создание резервной копии");

				if(currentTable != e.CurrentTableName)
					OperationProgress.Start(e.TotalRowsInCurrentTable, text: $"Экспорт {e.CurrentTableName}");

				OperationProgress.Update(e.CurrentRowIndexInCurrentTable);
				TotalProgress.Update(e.CurrentRowIndexInAllTables);
			});
		}
		#endregion

		#region Обновление
		void RunOneUpdate(UpdateHop updateScript)
		{
			var operationName = "Обновление базы до " + updateScript.Destination.VersionToShortString();
			logger.Info(operationName);
			CommandsLog += operationName + "\n";

			if(updateScript.ExecuteBefore != null) {
				CommandsLog += "Выполнение программный действий...\n";
				updateScript.ExecuteBefore(SQLProvider.DbConnection);
			}

			string sql;
			using (Stream stream = updateScript.Assembly.GetManifestResourceStream(updateScript.Resource)) {
				if (stream == null)
					throw new InvalidOperationException(String.Format("Ресурс {0} указанный в обновлениях не найден.", updateScript.Resource));
				StreamReader reader = new StreamReader(stream);
				sql = reader.ReadToEnd();
			}

			int predictedCount = Regex.Matches(sql, ";").Count;

			logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);

			OperationProgress.Start(text: operationName, maxValue: predictedCount);

			var script = new MySqlScript(SQLProvider.DbConnection, sql);
			script.StatementExecuted += Script_StatementExecuted;
			var commands = script.ExecuteAsync(cancellation.Token);
			guiDispatcher.WaitInMainLoop(() => commands.Status != System.Threading.Tasks.TaskStatus.Running, 50);
			logger.Debug("Выполнено {0} SQL-команд.", commands.Result);
			parametersService.ReloadParameters();
			parametersService.version = updateScript.Destination.VersionToShortString();
		}

		void Script_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			OperationProgress.Add();
			CommandsLog += args.StatementText + "\n";
			logger.Debug(args.StatementText);
		}
		#endregion
		#endregion

		public void OnClose(CloseSource source)
		{
			cancellation.Cancel();
		}
	}
}