using System;
using System.IO;
using System.Threading;
using MySqlConnector;
using QS.Dialog;

namespace QS.DbManagement {
	public class MariaDbDumpService : IDbDumpService {
		/// <summary>Выгружает базу <paramref name="databaseName"/> в файл <paramref name="filePath"/></summary>
		public void Export(
			string connectionString,
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

			RunWithBackup(connectionString, databaseName, backup => {
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
			string connectionString,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation,
			string title = null)
		{
			SqlDumpFileValidator.EnsureLooksLikeSqlDump(filePath);

			progress?.Update($"Импортируем дамп {filePath} в базу {databaseName}");

			RunWithBackup(connectionString, databaseName, backup => {
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

				if(!string.IsNullOrEmpty(title))
				{
					progress?.Update("Вставляем BaseTitle");
					backup.Command.CommandText = @"INSERT INTO base_parameters (name, str_value) 
						VALUES ('BaseTitle', @title)
						ON DUPLICATE KEY UPDATE
							str_value = VALUES(str_value);";
					backup.Command.Parameters.Clear();
					backup.Command.Parameters.AddWithValue("@title", title);
					backup.Command.ExecuteNonQuery();
					progress?.Update("Новый BaseTitle вставлен");
				}
			});
		}

		private void RunWithBackup(string connectionString, string databaseName, Action<MySqlBackup> action) {
			if(string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Не указана строка подключения", nameof(connectionString));
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentException("Не указано имя базы", nameof(databaseName));

			var builder = new MySqlConnectionStringBuilder(connectionString) {
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
