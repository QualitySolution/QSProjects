using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal static class LauncherColumnMapper
	{
		public static List<string> TableColumns(MySqlConnection connection, string schema, string table, MySqlTransaction tx = null)
		{
			return connection.Query<string>(
				"SELECT COLUMN_NAME FROM information_schema.COLUMNS " +
				"WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table ORDER BY ORDINAL_POSITION",
				new { schema, table }, tx).ToList();
		}

		public static Dictionary<string, List<string>> TableColumnsMany(MySqlConnection connection, IEnumerable<string> schemas, string table)
		{
			var wanted = schemas.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			if(!wanted.Any())
				return result;

			var rows = connection.Query<(string Schema, string Column)>(
				"SELECT TABLE_SCHEMA, COLUMN_NAME FROM information_schema.COLUMNS " +
				"WHERE TABLE_NAME = @table AND TABLE_SCHEMA IN @schemas ORDER BY TABLE_SCHEMA, ORDINAL_POSITION",
				new { table, schemas = wanted });

			foreach(var row in rows) {
				if(!result.TryGetValue(row.Schema, out var columns)) {
					columns = new List<string>();
					result[row.Schema] = columns;
				}
				columns.Add(row.Column);
			}
			return result;
		}

		public static HashSet<string> KeyColumns(MySqlConnection connection, string schema, string table, MySqlTransaction tx = null)
		{
			return new HashSet<string>(
				connection.Query<string>(
					"SELECT DISTINCT COLUMN_NAME FROM information_schema.STATISTICS " +
					"WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table",
					new { schema, table }, tx),
				StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// список колонок, у которых есть одноимённое свойство сущности
		/// </summary>
		public static string SelectList(IEnumerable<string> tableColumns, Type entityType)
		{
			var props = Properties(entityType); //?
			return string.Join(", ", tableColumns
				.Where(c => props.ContainsKey(Normalize(c)))
				.Select(c => $"`{c}` AS `{props[Normalize(c)].Name}`"));
		}

		public static string SelectListAligned(IEnumerable<string> tableColumns, Type entityType)
		{
			var byName = tableColumns
				.GroupBy(Normalize, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			return string.Join(", ", Properties(entityType).Values
				.OrderBy(p => p.Name, StringComparer.Ordinal)
				.Select(p => byName.TryGetValue(Normalize(p.Name), out var column)
					? $"`{column}` AS `{p.Name}`"
					: $"NULL AS `{p.Name}`"));
		}

		public static (List<string> columns, DynamicParameters parameters) MapForWrite(IEnumerable<string> tableColumns, object entity, ISet<string> exclude = null)
		{
			var props = Properties(entity.GetType());
			var columns = new List<string>();
			var parameters = new DynamicParameters();
			foreach(var column in tableColumns) {
				if(exclude != null && exclude.Contains(column))
					continue;
				if(!props.TryGetValue(Normalize(column), out var prop))
					continue;
				object value = prop.GetValue(entity);
				if(value is string s && s.Length == 0)
					value = null;
				columns.Add(column);
				parameters.Add(column, value);
			}
			return (columns, parameters);
		}

		private static string Normalize(string name)
			=> name.Replace("_", "").ToLowerInvariant();

		private static Dictionary<string, PropertyInfo> Properties(Type type)
			=> type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead)
				.GroupBy(p => Normalize(p.Name), StringComparer.Ordinal)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
	}
}
