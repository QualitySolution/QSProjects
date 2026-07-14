using MySqlConnector;
using QS.Dialog;
using System;
using System.IO;
using System.Threading;

namespace QS.DbManagement {
	public class MariaDbExportService {
		/// <summary>Выгружает базу <paramref name="databaseName"/> в файл <paramref name="filePath"/></summary>
		public void Export(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation) {
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу резервной копии.", nameof(filePath));

			var directory = Path.GetDirectoryName(filePath);
			if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			progress?.Update($"Создаём резервную копию базы {databaseName} в файл {filePath}");
			if(connectionSettings == null)
				throw new ArgumentNullException(nameof(connectionSettings));
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentException("Не указано имя базы", nameof(databaseName));

			var builder = new MySqlConnectionStringBuilder(connectionSettings.ConnectionString) {
				Database = databaseName
			};

			using(var connection = new MySqlConnection(builder.ConnectionString)) {
				connection.Open();
				using(var command = connection.CreateCommand()) {
					command.CommandTimeout = 0;
					using(var backup = new MySqlBackup(command)) {

						bool started = false;
						string currentTable = null;
						backup.ExportProgressChanged += (sender, e) => {
							if(cancellation.IsCancellationRequested) {
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
						};
						backup.ExportToFile(filePath);
					}
				}
			}
		}
	}
}
