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

		/// <summary>
		/// Проекция под свойства сущности с одинаковым набором и порядком колонок для любой базы - иначе ветки UNION не соединить
		/// Недостающие колонки подставляются как NULL
		/// </summary>
		public static string SelectListAligned(IEnumerable<string> tableColumns, Type entityType)
		{
			var byName = new NameIndex<string>(tableColumns, c => c);

			return string.Join(", ", Properties(entityType).Items
				.OrderBy(p => p.Name, StringComparer.Ordinal)
				.Select(p => byName.TryFind(p.Name, out var column)
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
				if(!props.TryFind(column, out var prop))
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

		private static NameIndex<PropertyInfo> Properties(Type type) //?
			=> new NameIndex<PropertyInfo>(
				type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead),
				p => p.Name);

		private sealed class NameIndex<T> where T : class
		{
			private readonly Dictionary<string, T> byExactName;
			private readonly Dictionary<string, T> byNormalizedName;

			public NameIndex(IEnumerable<T> items, Func<T, string> nameOf) {
				var list = items.ToList();
				byExactName = ToDictionary(list, i => nameOf(i));
				byNormalizedName = ToDictionary(list, i => Normalize(nameOf(i)));
			}

			public IEnumerable<T> Items => byExactName.Values;

			public bool TryFind(string name, out T item)
				=> byExactName.TryGetValue(name, out item)
					|| byNormalizedName.TryGetValue(Normalize(name), out item);

			private static Dictionary<string, T> ToDictionary(IEnumerable<T> items, Func<T, string> keyOf)
				=> items.GroupBy(keyOf, StringComparer.OrdinalIgnoreCase)
					.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
		}
	}
}
