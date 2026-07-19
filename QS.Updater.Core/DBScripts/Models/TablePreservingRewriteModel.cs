using QS.DBScripts.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QS.DBScripts.Models {
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

	public class TablePreservingRewriteModel : IDbRewriteModel {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IEnumerable<PreservedTable> tables;
		private readonly Dictionary<PreservedTable, List<Dictionary<string, object>>> saved =
			new Dictionary<PreservedTable, List<Dictionary<string, object>>>();

		public TablePreservingRewriteModel(IEnumerable<PreservedTable> tables) {
			if(tables == null)
				throw new ArgumentNullException(nameof(tables));
			this.tables = tables;
		}

		public bool Backup(IDbCommand cmd) {
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

		public void Restore(IDbCommand cmd) {
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
						AddParameter(cmd, $"@p{i}", row.TryGetValue(columns[i], out var value) ? value : null);

					if(useKey) {
						// строки с тем же ключом уже пришли из наполнения (например из дампа) - их не перезаписываем
						AddParameter(cmd, "@key", row[table.KeyColumn]);
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

		private static bool TableExists(IDbCommand cmd, string tableName) {
			cmd.Parameters.Clear();
			AddParameter(cmd, "@table", tableName);
			cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @table";
			return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
		}

		private static List<string> GetColumns(IDbCommand cmd, string tableName) {
			cmd.Parameters.Clear();
			AddParameter(cmd, "@table", tableName);
			cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table";
			var result = new List<string>();
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read())
					result.Add(rdr.GetString(0));
			}
			return result;
		}

		private static void AddParameter(IDbCommand cmd, string name, object value) {
			var parameter = cmd.CreateParameter();
			parameter.ParameterName = name;
			parameter.Value = value ?? DBNull.Value;
			cmd.Parameters.Add(parameter);
		}

		private static string EscapeIdentifier(string value) => value.Replace("`", "``");
	}
}
