using System;
using System.IO;
using System.Threading;
using MySqlConnector;
using QS.Dialog;

namespace QS.DbManagement {
	/// <summary>
	/// Экспорт базы MariaDB/MySQL в SQL-скрипт через MySqlBackup.NET.
	/// Вынесен отдельным сервисом, чтобы провайдер не держал логику бэкапа в себе.
	/// </summary>
	public class MariaDbBackupService {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Синхронно выгружает базу <paramref name="databaseName"/> в файл <paramref name="filePath"/>.
		/// Вызывать из фонового потока - метод блокирующий (как и MySqlBackup).
		/// </summary>
		public void Backup(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation) {
			if(connectionSettings == null)
				throw new ArgumentNullException(nameof(connectionSettings));
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentException("Не указано имя базы для резервного копирования.", nameof(databaseName));
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу резервной копии.", nameof(filePath));

			// Отдельная строка подключения именно к выгружаемой базе - провайдер может быть подключён к другой.
			var builder = new MySqlConnectionStringBuilder(connectionSettings.ConnectionString) {
				Database = databaseName
			};

			var directory = Path.GetDirectoryName(filePath);
			if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			logger.Info("Создаём резервную копию базы {0} в файл {1}", databaseName, filePath);

			using(var connection = new MySqlConnection(builder.ConnectionString)) {
				connection.Open();
				using(var command = connection.CreateCommand())
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
