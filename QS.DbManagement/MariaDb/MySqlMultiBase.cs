using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QS.DbManagement.MariaDb {
	/// <summary>
	/// Запросы к одной и той же таблице сразу во многих базах одного сервера.
	/// Смысл в том, чтобы не подключаться к каждой базе по отдельности: своя база в строке
	/// подключения - это свой пул, а на сервере с обратным резолвом имён каждое новое
	/// подключение стоит дорого. Всё делается по уже открытому серверному соединению.
	/// </summary>
	internal static class MySqlMultiBase {
		/// <summary>
		/// Колонки таблицы в каждой из баз. Базы, где таблицы нет или куда нет доступа,
		/// в результат не попадают - заодно это и ответ на вопрос, в каких базах она есть.
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
		/// Один SELECT вместо запроса на каждую базу: ветки склеиваются UNION ALL, и каждая
		/// помечает свои строки именем своей базы - иначе в общем результате их не различить.
		/// Число и порядок колонок в ветках обязаны совпадать, за это отвечает вызывающий:
		/// на разъехавшихся ветках UNION не собирается.
		/// </summary>
		/// <param name="projections">база -> список колонок её ветки</param>
		/// <param name="label">имя колонки-метки, в неё уходит имя базы</param>
		/// <param name="where">условие ветки без слова WHERE; null - без условия</param>
		/// <param name="parameters">сюда добавляются метки баз, значения условия кладёт вызывающий</param>
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
