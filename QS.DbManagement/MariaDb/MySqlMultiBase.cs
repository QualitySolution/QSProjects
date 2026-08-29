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
		public static List<string> DatabasesWithTable(IDbConnection connection, IEnumerable<string> databases, string table) {
			var wanted = Distinct(databases);
			if(wanted.Count == 0)
				return new List<string>();

			return connection.Query<string>(
				"SELECT TABLE_SCHEMA FROM information_schema.TABLES " +
				"WHERE TABLE_NAME = @table AND TABLE_SCHEMA IN @databases ORDER BY TABLE_SCHEMA",
				new { table, databases = wanted }).ToList();
		}

		/// <summary>Есть ли в базе такая таблица - и видна ли она текущему пользователю</summary>
		public static bool HasTable(IDbConnection connection, string database, string table)
			=> DatabasesWithTable(connection, new[] { database }, table).Count > 0;

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
	}
}
