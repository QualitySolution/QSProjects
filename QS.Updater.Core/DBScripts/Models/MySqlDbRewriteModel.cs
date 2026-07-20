using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DBScripts.Models {
	/// <summary>
	/// Таблица, данные которой должны пережить перезапись базы
	/// </summary>
	public class PreservedTable {
		public PreservedTable(string tableName, string keyColumn = null, params string[] skipColumns) {
			if(string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentException("Не указано имя таблицы", nameof(tableName));
			TableName = tableName;
			KeyColumn = keyColumn;
			SkipColumns = skipColumns ?? Array.Empty<string>();
		}

		public string TableName { get; }
		/// <summary>Строки, чьё значение ключа уже есть после наполнения (например пришло из дампа), не восстанавливаются</summary>
		public string KeyColumn { get; }
		/// <summary>Колонки, которые не переносятся - например автоинкрементный id, чтобы не столкнуться со строками из наполнения</summary>
		public IReadOnlyList<string> SkipColumns { get; }
	}

	public class MySqlDbRewriteModel : IDbRewriteModel {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly string connectionString;
		private readonly IProgressBarDisplayable progress;
		private readonly IDbCreatorInteraction interaction;
		private readonly IReadOnlyList<PreservedTable> tables;

		private readonly Dictionary<PreservedTable, List<Dictionary<string, object>>> saved =
			new Dictionary<PreservedTable, List<Dictionary<string, object>>>();

		public MySqlDbRewriteModel(DbCreationResources resources, IEnumerable<PreservedTable> preservedTables) {
			if(resources == null)
				throw new ArgumentNullException(nameof(resources));
			if(string.IsNullOrWhiteSpace(resources.ConnectionString))
				throw new ArgumentException("Connection string is required", nameof(resources));
			this.connectionString = resources.ConnectionString;
			this.progress = resources.Progress ?? throw new ArgumentNullException(nameof(resources.Progress));
			this.interaction = resources.Interactions ?? throw new ArgumentNullException(nameof(resources.Interactions));
			this.tables = preservedTables?.ToList() ?? new List<PreservedTable>();
		}

		public bool RunRewrite(IDbCreatorModel creationModel, string dbName, string dbTitle) {
			if(creationModel == null)
				throw new ArgumentNullException(nameof(creationModel));

			bool hasPreservedData;
			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var cmd = new MySqlCommand(connection, null);
					cmd.CommandText = $"USE `{EscapeIdentifier(dbName)}` ;";
					cmd.ExecuteNonQuery();

					progress.Add(text: "Сохраняем данные существующей базы");
					hasPreservedData = Backup(cmd);

					// если сохранять нечего, перезапись эквивалентна пересозданию и версия не важна
					if(hasPreservedData && !VersionMatches(cmd, creationModel.NewBaseVersion, out string versionError)) {
						interaction.ReportError(versionError, null);
						return false;
					}

					DropAllSchemaObjects(cmd);
				}
			}
			catch(MySqlException ex) {
				logger.Error(ex, "Ошибка при подготовке базы {0} к перезаписи.", dbName);
				interaction.ReportError(ex.Message, null);
				return false;
			}

			if(!creationModel.RunCreation(dbName, dbTitle))
				return false;

			if(!hasPreservedData)
				return true;

			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var cmd = new MySqlCommand(connection, null);
					cmd.CommandText = $"USE `{EscapeIdentifier(dbName)}` ;";
					cmd.ExecuteNonQuery();

					progress.Add(text: "Восстанавливаем сохранённые данные");
					Restore(cmd);
				}
			}
			catch(MySqlException ex) {
				logger.Error(ex, "Ошибка при восстановлении сохранённых данных в базе {0}.", dbName);
				interaction.ReportError("База наполнена, но восстановить сохранённые данные не удалось:\n" + ex.Message, null);
				return false;
			}
			return true;
		}

		private bool VersionMatches(MySqlCommand cmd, Version newVersion, out string error) {
			error = null;
			if(newVersion == null)
				return true;

			string current = null;
			try {
				cmd.Parameters.Clear();
				cmd.CommandText = "SELECT str_value FROM base_parameters WHERE name = 'version'";
				current = cmd.ExecuteScalar() as string;
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось прочитать версию существующей базы.");
			}

			if(current == null || !Version.TryParse(current, out var currentVersion)) {
				error = "Не удалось определить версию существующей базы.\nПерезапись с сохранением данных невозможна.";
				return false;
			}
			if(currentVersion.Major != newVersion.Major || currentVersion.Minor != newVersion.Minor) {
				error = $"Версия существующей базы ({currentVersion}) не совпадает с версией создаваемой ({newVersion.ToString(2)}).\n" +
					"Сначала обновите существующую базу или пересоздайте её полностью.";
				return false;
			}
			return true;
		}

		/// <returns>true - есть данные, которые нужно вернуть после наполнения</returns>
		private bool Backup(MySqlCommand cmd) {
			saved.Clear();
			foreach(var table in tables) {
				if(!TableExists(cmd, table.TableName))
					continue;
				logger.Info("Сохраняем данные таблицы {0}.", table.TableName);
				var rows = new List<Dictionary<string, object>>();
				cmd.Parameters.Clear();
				cmd.CommandText = $"SELECT * FROM `{EscapeIdentifier(table.TableName)}`";
				using(var rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
						for(int i = 0; i < rdr.FieldCount; i++)
							row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
						rows.Add(row);
					}
				}
				if(rows.Count > 0)
					saved[table] = rows;
			}
			return saved.Count > 0;
		}

		private void Restore(MySqlCommand cmd) {
			foreach(var pair in saved) {
				var table = pair.Key;
				var rows = pair.Value;

				var newColumns = GetColumns(cmd, table.TableName);
				if(newColumns.Count == 0) {
					logger.Warn("Наполнение не создало таблицу {0} - восстанавливать данные некуда.", table.TableName);
					continue;
				}

				// пересечение колонок переживает изменение схемы таблицы между наполнениями
				var columns = rows[0].Keys
					.Intersect(newColumns, StringComparer.OrdinalIgnoreCase)
					.Where(c => !table.SkipColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
					.ToList();
				if(columns.Count == 0)
					continue;
				bool useKey = table.KeyColumn != null && columns.Contains(table.KeyColumn, StringComparer.OrdinalIgnoreCase);

				logger.Info("Восстанавливаем {0} строк таблицы {1}.", rows.Count, table.TableName);
				string tableName = $"`{EscapeIdentifier(table.TableName)}`";
				string columnList = string.Join(", ", columns.Select(c => $"`{EscapeIdentifier(c)}`"));
				string valueList = string.Join(", ", columns.Select((c, i) => $"@p{i}"));

				foreach(var row in rows) {
					cmd.Parameters.Clear();
					for(int i = 0; i < columns.Count; i++)
						cmd.Parameters.AddWithValue($"@p{i}", row.TryGetValue(columns[i], out var value) ? value ?? DBNull.Value : DBNull.Value);

					if(useKey) {
						// строки с тем же ключом уже пришли из наполнения (например из дампа) - их не перезаписываем
						cmd.Parameters.AddWithValue("@key", row[table.KeyColumn] ?? DBNull.Value);
						cmd.CommandText = $"INSERT INTO {tableName} ({columnList}) " +
							$"SELECT {valueList} FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM {tableName} WHERE `{EscapeIdentifier(table.KeyColumn)}` = @key)";
					}
					else {
						cmd.CommandText = $"INSERT INTO {tableName} ({columnList}) VALUES ({valueList})";
					}
					cmd.ExecuteNonQuery();
				}
				cmd.Parameters.Clear();
			}
		}

		private void DropAllSchemaObjects(MySqlCommand cmd) {
			logger.Info("Удаляем все объекты существующей базы.");
			progress.Add(text: "Очищаем существующую базу");
			cmd.Parameters.Clear();

			var dropTables = new List<string>();
			var views = new List<string>();
			cmd.CommandText = "SELECT table_name, table_type FROM information_schema.tables WHERE table_schema = DATABASE()";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read()) {
					if("VIEW".Equals(rdr.GetString(1), StringComparison.OrdinalIgnoreCase))
						views.Add(rdr.GetString(0));
					else
						dropTables.Add(rdr.GetString(0));
				}
			}

			// процедуры и функции таблицам не принадлежат и сами не удалятся,
			// а повторный CREATE из наполнения на существующей упадёт
			var routines = new List<(string Name, string Type)>();
			cmd.CommandText = "SELECT routine_name, routine_type FROM information_schema.routines WHERE routine_schema = DATABASE()";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read())
					routines.Add((rdr.GetString(0), rdr.GetString(1)));
			}

			cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 0";
			cmd.ExecuteNonQuery();
			if(views.Count > 0) {
				cmd.CommandText = "DROP VIEW IF EXISTS " + string.Join(", ", views.Select(v => $"`{EscapeIdentifier(v)}`"));
				cmd.ExecuteNonQuery();
			}
			if(dropTables.Count > 0) {
				cmd.CommandText = "DROP TABLE IF EXISTS " + string.Join(", ", dropTables.Select(t => $"`{EscapeIdentifier(t)}`"));
				cmd.ExecuteNonQuery();
			}
			foreach(var routine in routines) {
				cmd.CommandText = $"DROP {routine.Type} IF EXISTS `{EscapeIdentifier(routine.Name)}`";
				cmd.ExecuteNonQuery();
			}
			cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 1";
			cmd.ExecuteNonQuery();
		}

		private static bool TableExists(MySqlCommand cmd, string tableName) {
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@table", tableName);
			cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @table";
			return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
		}

		private static List<string> GetColumns(MySqlCommand cmd, string tableName) {
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@table", tableName);
			cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table";
			var result = new List<string>();
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read())
					result.Add(rdr.GetString(0));
			}
			return result;
		}

		private static string EscapeIdentifier(string value) => value.Replace("`", "``");
	}
}
