using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Versioning;
using QS.Updater.DB;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace DB.ViewModels
{
	public class UpdateProcessViewModel : WindowDialogViewModelBase
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public bool Success { get; private set; }

		private readonly UpdateConfiguration configuration;
		IMySQLProvider SQLProvider;
		private readonly IApplicationInfo applicationInfo;
		private readonly dynamic parametersService;
		private readonly IInteractiveMessage interactive;
		private readonly IGuiDispatcher guiDispatcher;

		private UpdateHop[] hops;
		Version dbVersion;

		public UpdateProcessViewModel(
			INavigationManager navigation,
			UpdateConfiguration configuration,
			IMySQLProvider mySQLProvider,
			IApplicationInfo applicationInfo,
			ParametersService parametersService,
			IInteractiveMessage interactive,
			IGuiDispatcher guiDispatcher) : base(navigation)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			SQLProvider = mySQLProvider;
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			FileName = System.IO.Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"Резервные копии",
				String.Format("{0}{1:yyMMdd-HHmm}.sql", applicationInfo.ProductName, DateTime.Now)
			);

			dbVersion = GetCurrentDBVersion();
			hops = configuration.GetHopsToLast(dbVersion).ToArray();
		}

		#region Свойства для View

		private string fileName;
		public virtual string FileName {
			get => fileName;
			set => SetField(ref fileName, value);
		}

		private bool needCreateBackup;
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

		private IProgressBarDisplayable totalProgress;
		public IProgressBarDisplayable TotalProgress {
			get => totalProgress; set {
				totalProgress = value;
				if(value != null) {
					TotalProgress.Start(0, text: String.Format("Обновление: {0} → {1}",
						VersionHelper.VersionToShortString(dbVersion),
						VersionHelper.VersionToShortString(hops.Last().Destanation)
					));
				}
			}
		}

		private bool buttonExcuteSensitive = true;
		public virtual bool ButtonExcuteSensitive {
			get => buttonExcuteSensitive;
			set => SetField(ref buttonExcuteSensitive, value);
		}

		#endregion
		#region Действия View

		public void Execute()
		{
			try {
				if (NeedCreateBackup) {
					if (ExecuteBackup()) {
						return;
					}
				}

				TotalProgress.Start(maxValue: hops.Length, text: String.Format("Обновление: {0} → {1}",
						VersionHelper.VersionToShortString(dbVersion),
						VersionHelper.VersionToShortString(hops.Last().Destanation)
					));
				foreach (var hop in hops) {
					RunOneUpdate(hop);
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
			Close(false, CloseSource.Cancel);
		}

		#endregion

		#region Внутреннее

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

			while (bwExport.IsBusy) {
				System.Threading.Thread.Sleep(50);
				guiDispatcher.WaitRedraw();
			}

			OperationProgress.Close();
			return false;
		}

		void RunOneUpdate(UpdateHop updateScript)
		{
			var operationName = (updateScript.UpdateType == UpdateType.MicroUpdate ? "Микро-обновление" : "Обновление")
				+ $" базы до " + updateScript.Destanation.VersionToShortString();
			logger.Info(operationName);
			CommandsLog += operationName + "\n";

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
			var commands = script.Execute();
			logger.Debug("Выполнено {0} SQL-команд.", commands);
		}

		void BwExport_DoWork(object sender, DoWorkEventArgs e)
		{
			using (MySqlCommand cmd = SQLProvider.DbConnection.CreateCommand()) {
				using (MySqlBackup mb = new MySqlBackup(cmd)) {
					var dir = System.IO.Path.GetDirectoryName(FileName);

					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					logger.Debug(FileName);
					currentTable = null;
					mb.ExportProgressChanged += Mb_ExportProgressChanged;

					mb.ExportToFile(FileName);
				}
			}
		}

		void Script_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			OperationProgress.Add();
			commandsLog += args.StatementText + "\n";
			logger.Debug(args.StatementText);
		}

		private string currentTable;
		void Mb_ExportProgressChanged(object sender, ExportProgressArgs e)
		{
			guiDispatcher.RunInGuiTread( delegate {
				if (currentTable == null)
					totalProgress.Start(maxValue: e.TotalRowsInAllTables, text: "Создание резервной копии");

				if (currentTable != e.CurrentTableName)
					OperationProgress.Start(e.TotalRowsInCurrentTable, text: $"Экспорт {e.CurrentTableName}");

				OperationProgress.Update(e.CurrentRowIndexInCurrentTable);
				totalProgress.Update(e.CurrentRowIndexInAllTables);
			});
		}

		#endregion

		#region DbVersion

		Version GetCurrentDBVersion()
		{
			var versionString = parametersService.micro_updates ?? parametersService.version;
			return Version.Parse(versionString);
		}

		#endregion
	}
}
