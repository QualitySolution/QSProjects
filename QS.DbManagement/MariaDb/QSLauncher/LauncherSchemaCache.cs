using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal sealed class LauncherSchemaCache {
		private readonly ConcurrentDictionary<string, List<string>> columnsByTable =
			new ConcurrentDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<string, HashSet<string>> keysByTable =
			new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		public List<string> TableColumns(MySqlConnection connection, string schema, string table, MySqlTransaction tx = null) {
			var cached = Cached(columnsByTable, schema, table,
				() => LauncherColumnMapper.TableColumns(connection, schema, table, tx));
			return new List<string>(cached);
		}

		public HashSet<string> KeyColumns(MySqlConnection connection, string schema, string table, MySqlTransaction tx = null) {
			var cached = Cached(keysByTable, schema, table,
				() => LauncherColumnMapper.KeyColumns(connection, schema, table, tx));
			return new HashSet<string>(cached, StringComparer.OrdinalIgnoreCase);
		}

		private static T Cached<T>(ConcurrentDictionary<string, T> store, string schema, string table, Func<T> read)
			where T : class, ICollection<string> {
			string key = schema + "." + table;
			if(store.TryGetValue(key, out var cached))
				return cached;

			var value = read();
			if(value.Count > 0) //таблицы может ещё не быть - метабазу собирают по ходу работы
				store.TryAdd(key, value);
			return value;
		}
	}
}
