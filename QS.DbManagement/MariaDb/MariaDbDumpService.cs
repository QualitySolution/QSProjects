using System;
using System.IO;
using System.Threading;
using MySqlConnector;
using QS.Dialog;

namespace QS.DbManagement {
	public class MariaDbDumpService {
		/// <summary>Выгружает базу <paramref name="databaseName"/> в файл <paramref name="filePath"/></summary>
		public void Export(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation)
		{
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу резервной копии.", nameof(filePath));

			var directory = Path.GetDirectoryName(filePath);
			if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			progress?.Update($"Создаём резервную копию базы {databaseName} в файл {filePath}");

			RunWithBackup(connectionSettings, databaseName, backup => {
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
			});
		}

		/// <summary>Заливает дамп <paramref name="filePath"/> в уже существующую базу <paramref name="databaseName"/></summary>
		public void Import(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation) {
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу дампа.", nameof(filePath));
			if(!File.Exists(filePath))
				throw new FileNotFoundException("Файл дампа не найден.", filePath);

			progress?.Update($"Импортируем дамп {filePath} в базу {databaseName}");

			RunWithBackup(connectionSettings, databaseName, backup => {
				bool started = false;
				backup.ImportProgressChanged += (sender, e) => {
					if(cancellation.IsCancellationRequested) {
						((MySqlBackup)sender).StopAllProcess();
						return;
					}
					if(!started) {
						progress?.Start(maxValue: e.TotalBytes, text: "Импорт дампа в базу данных");
						started = true;
					}
					progress?.Update(e.CurrentBytes);
				};
				backup.ImportFromFile(filePath);
			});
		}

		private void RunWithBackup(MySqlConnectionStringBuilder connectionSettings, string databaseName, Action<MySqlBackup> action) {
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
					using(var backup = new MySqlBackup(command)) {
						action(backup);
					}
				}
			}
		}
	}
}
