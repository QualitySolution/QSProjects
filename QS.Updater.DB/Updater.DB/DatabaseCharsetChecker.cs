using System.Collections.Generic;
using System.Threading;
using MySqlConnector;
using QS.Dialog;

namespace QS.Updater.DB
{
	/// <summary>
	/// Проверяет и исправляет кодировку и сортировку всех таблиц базы данных.
	/// </summary>
	public class DatabaseCharsetChecker
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Включить или отключить проверку кодировки. По умолчанию — включена.
		/// </summary>
		public bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Целевая кодировка таблиц. По умолчанию utf8mb4.
		/// </summary>
		public string CharacterSet { get; set; } = "utf8mb4";

		/// <summary>
		/// Целевое сравнение (collation) таблиц. По умолчанию utf8mb4_general_ci.
		/// </summary>
		public string Collation { get; set; } = "utf8mb4_general_ci";

		/// <summary>
		/// Проверяет все таблицы базы данных и конвертирует те, у которых кодировка/collation
		/// не соответствует <see cref="CharacterSet"/> / <see cref="Collation"/>.
		/// </summary>
		/// <param name="connection">Открытое соединение с базой данных.</param>
		/// <param name="progress">Прогресс-бар для отображения хода операции (может быть null).</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Количество исправленных таблиц.</returns>
		public int FixTablesCharset(MySqlConnection connection, IProgressBarDisplayable progress = null, CancellationToken cancellationToken = default)
		{
			if (!IsEnabled) {
				logger.Info("Проверка кодировки таблиц отключена.");
				return 0;
			}

			logger.Info("Проверка кодировки таблиц базы данных ({0} / {1}).", CharacterSet, Collation);

			var tablesToFix = GetTablesWithWrongCharset(connection);

			if (tablesToFix.Count == 0) {
				logger.Info("Все таблицы уже имеют правильную кодировку.");
				return 0;
			}

			logger.Info("Найдено {0} таблиц с неправильной кодировкой.", tablesToFix.Count);

			progress?.Start(maxValue: tablesToFix.Count, text: $"Исправление кодировки таблиц ({CharacterSet} / {Collation})");

			int fixedCount = 0;
			foreach (var tableName in tablesToFix) {
				if (cancellationToken.IsCancellationRequested)
					break;

				logger.Debug("Конвертируем таблицу `{0}`.", tableName);
				progress?.Update($"Конвертируем таблицу {tableName}");

				using (var cmd = connection.CreateCommand()) {
					cmd.CommandText = $"ALTER TABLE `{tableName}` CONVERT TO CHARACTER SET {CharacterSet} COLLATE {Collation};";
					cmd.ExecuteNonQuery();
				}

				fixedCount++;
				progress?.Add();
			}

			progress?.Close();
			logger.Info("Исправлено {0} таблиц.", fixedCount);
			return fixedCount;
		}

		/// <summary>
		/// Возвращает список таблиц, у которых collation не соответствует <see cref="Collation"/>.
		/// </summary>
		private List<string> GetTablesWithWrongCharset(MySqlConnection connection)
		{
			var tables = new List<string>();
			var dbName = connection.Database;

			using (var cmd = connection.CreateCommand()) {
				cmd.CommandText =
					"SELECT TABLE_NAME " +
					"FROM INFORMATION_SCHEMA.TABLES " +
					"WHERE TABLE_SCHEMA = @db " +
					"  AND TABLE_TYPE = 'BASE TABLE' " +
					"  AND TABLE_COLLATION != @collation " +
					"ORDER BY TABLE_NAME;";
				cmd.Parameters.AddWithValue("@db", dbName);
				cmd.Parameters.AddWithValue("@collation", Collation);

				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read())
						tables.Add(reader.GetString(0));
				}
			}

			return tables;
		}
	}
}



