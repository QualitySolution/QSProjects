using MySqlConnector;
using QS.Dialog;
using System;
using System.IO;
using System.Threading;

namespace QS.DbManagement {
	public class MariaDbExportService {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>Выгружает базу <paramref name="databaseName"/> в файл <paramref name="filePath"/></summary>
		public void Export(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation)
		{
			if(connectionSettings == null)
				throw new ArgumentNullException(nameof(connectionSettings));
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentException("Не указано имя базы", nameof(databaseName));
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу резервной копии.", nameof(filePath));

			EnsureDirectory(filePath);
			progress?.Update($"Создаём резервную копию базы {databaseName} в файл {filePath}");

			var builder = new MySqlConnectionStringBuilder(connectionSettings.ConnectionString)
			{
				Database = databaseName
			};

			using(var connection = new MySqlConnection(builder.ConnectionString))
			{
				connection.Open();
				using(var command = connection.CreateCommand())
				{
					command.CommandTimeout = 0;
					using(var backup = new MySqlBackup(command))
					{
						var reporter = new ExportReporter(progress, cancellation);
						backup.ExportProgressChanged += reporter.OnProgress;
						backup.ExportToFile(filePath);

						if(reporter.Stopped || cancellation.IsCancellationRequested)
						{
							DeleteQuietly(filePath);
							cancellation.ThrowIfCancellationRequested();
						}
					}
				}
			}
		}

		private static void EnsureDirectory(string filePath)
		{
			var directory = Path.GetDirectoryName(filePath);
			if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);
		}

		private sealed class ExportReporter
		{
			private readonly IProgressBarDisplayable progress;
			private readonly CancellationToken cancellation;
			private bool started;
			private string currentTable;

			public ExportReporter(IProgressBarDisplayable progress, CancellationToken cancellation)
			{
				this.progress = progress;
				this.cancellation = cancellation;
			}

			/// <summary>Выгрузку остановили по отмене - файл дописан не до конца</summary>
			public bool Stopped { get; private set; }

			public void OnProgress(object sender, ExportProgressArgs e)
			{
				if(cancellation.IsCancellationRequested) {
					Stopped = true;
					((MySqlBackup)sender).StopAllProcess();
					return;
				}

				if(!started) {
					progress?.Start(maxValue: e.TotalRowsInAllTables, text: "Создание резервной копии");
					started = true;
				}
				if(currentTable != e.CurrentTableName) {
					currentTable = e.CurrentTableName;
					progress?.Update($"Экспорт таблицы {currentTable}");
				}
				progress?.Update(e.CurrentRowIndexInAllTables);
			}
		}

		private static void DeleteQuietly(string filePath)
		{
			try {
				if(File.Exists(filePath))
					File.Delete(filePath);
			}
			catch(IOException ex) {
				logger.Warn(ex, "Не удалось удалить незавершённую резервную копию {0}", filePath);
			}
		}
	}
}
