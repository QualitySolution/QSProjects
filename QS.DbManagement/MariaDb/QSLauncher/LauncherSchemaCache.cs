using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QS.DbManagement.MariaDb.QSLauncher {
	/// <summary>
	/// Состав колонок метабазы за время работы лаунчера не меняется, поэтому
	/// information_schema читается по одному разу на таблицу, а не на каждую операцию.
	/// Экземпляр живёт вместе с подключением: на другом сервере метабаза может быть другой версии
	/// </summary>
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

		/// <summary>
		/// Пустой ответ не запоминаем: таблицы может ещё не быть - метабазу собирают по ходу работы,
		/// и запомненная пустота осталась бы с нами до перезапуска
		/// </summary>
		private static T Cached<T>(ConcurrentDictionary<string, T> store, string schema, string table, Func<T> read)
			where T : class, ICollection<string> {
			string key = schema + "." + table;
			if(store.TryGetValue(key, out var cached))
				return cached;

			var value = read();
			if(value.Count > 0)
				store.TryAdd(key, value);
			return value;
		}
	}
}
