using System;
using System.IO;
using System.Threading;
using MySqlConnector;
using QS.Dialog;

namespace QS.DbManagement {
	/// <summary>
	/// Импорт SQL-дампа в существующую базу MariaDB/MySQL через MySqlBackup.NET.
	/// Симметричен <see cref="MariaDbBackupService"/>. Вынесен отдельным сервисом,
	/// чтобы провайдер не держал логику работы с дампом в себе.
	/// </summary>
	public class MariaDbImportService {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Синхронно заливает дамп <paramref name="filePath"/> в базу <paramref name="databaseName"/>.
		/// База должна уже существовать. Вызывать из фонового потока - метод блокирующий.
		/// </summary>
		public void Import(
			MySqlConnectionStringBuilder connectionSettings,
			string databaseName,
			string filePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation) {
			if(connectionSettings == null)
				throw new ArgumentNullException(nameof(connectionSettings));
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentException("Не указано имя базы для импорта дампа.", nameof(databaseName));
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу дампа.", nameof(filePath));
			if(!File.Exists(filePath))
				throw new FileNotFoundException("Файл дампа не найден.", filePath);

			// Отдельная строка подключения именно к целевой базе - провайдер может смотреть в другую.
			var builder = new MySqlConnectionStringBuilder(connectionSettings.ConnectionString) {
				Database = databaseName
			};

			logger.Info("Импортируем дамп {0} в базу {1}", filePath, databaseName);

			using(var connection = new MySqlConnection(builder.ConnectionString)) {
				connection.Open();
				using(var command = connection.CreateCommand())
				using(var backup = new MySqlBackup(command)) {
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
				}
			}
		}
	}
}
