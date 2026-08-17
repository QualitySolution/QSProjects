using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QS.DbManagement.MariaDb {
	internal static class MySqlMultiBase {
		/// <summary>
		/// базы, где таблицы нет или куда нет доступа, в результат не попадают
		/// </summary>
		public static Dictionary<string, List<string>> TableColumns(IDbConnection connection, IEnumerable<string> databases, string table) {
			var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			var wanted = Distinct(databases);
			if(wanted.Count == 0)
				return result;

			var rows = connection.Query<ColumnRow>(
				"SELECT TABLE_SCHEMA AS BaseName, COLUMN_NAME AS ColumnName FROM information_schema.COLUMNS " +
				"WHERE TABLE_NAME = @table AND TABLE_SCHEMA IN @databases ORDER BY TABLE_SCHEMA, ORDINAL_POSITION",
				new { table, databases = wanted });

			foreach(var row in rows) {
				if(!result.TryGetValue(row.BaseName, out var columns)) {
					columns = new List<string>();
					result[row.BaseName] = columns;
				}
				columns.Add(row.ColumnName);
			}
			return result;
		}

		/// <summary>
		/// Число и порядок колонок в ветках обязаны совпадать
		/// </summary>
		/// <param name="projections">база -> список колонок её ветки</param>
		/// <param name="label">base_parameters имя перменной с именем базы</param>
		/// <param name="where">условие ветки без слова WHERE; null - без условия</param>
		/// <param name="parameters">сюда добавляются метки баз</param>
		public static string UnionAll(IEnumerable<KeyValuePair<string, string>> projections,
			string table, string label, string where, DynamicParameters parameters) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			string condition = string.IsNullOrEmpty(where) ? string.Empty : " WHERE " + where;
			string tableName = $"`{MySqlEscape.Identifier(table)}`";

			var selects = new List<string>();
			foreach(var projection in projections) {
				// имя базы уходит в запрос дважды: идентификатором в кавычках и меткой - параметром
				string marker = "base" + selects.Count.ToString();
				parameters.Add(marker, projection.Key);
				selects.Add($"SELECT @{marker} AS `{label}`, {projection.Value} "
					+ $"FROM `{MySqlEscape.Identifier(projection.Key)}`.{tableName}{condition}");
			}
			return string.Join(" UNION ALL ", selects);
		}

		/// <summary>Имена баз без повторов и пустых - в таком виде их принимают оба запроса</summary>
		public static List<string> Distinct(IEnumerable<string> databases) {
			if(databases == null)
				throw new ArgumentNullException(nameof(databases));

			return databases
				.Where(db => !string.IsNullOrEmpty(db))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private sealed class ColumnRow {
			public string BaseName { get; set; }
			public string ColumnName { get; set; }
		}
	}
}
