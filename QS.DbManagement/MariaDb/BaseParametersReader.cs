using Dapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace QS.DbManagement.MariaDb {
	/// <summary> Чтение base_parameters сразу из многих баз одного сервера по одному соединению </summary>
	internal static class BaseParametersReader {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private const string ParametersTable = "base_parameters";
		private const string BaseNameColumn = "BaseName";
		/// <summary>Колонки у base_parameters фиксированные - ветки различаются только базой</summary>
		private const string Projection = "`name` AS Name, `str_value` AS StrValue";
		/// <summary>Больше в один запрос класть незачем: дальше растёт только его длина</summary>
		private const int DefaultBatchSize = 50;

		/// <summary>
		/// Параметры перечисленных баз. Базы без таблицы base_parameters и базы, которые прочитать не удалось, в результат не попадают
		/// </summary>
		/// <param name="names">какие параметры нужны, null - все</param>
		/// <returns>base_name -> (name -> str_value)</returns>
		public static Dictionary<string, Dictionary<string, string>> ReadMany(
			IDbConnection connection, IEnumerable<string> databases,
			IEnumerable<string> names = null, int batchSize = DefaultBatchSize) {
			if(connection == null)
				throw new ArgumentNullException(nameof(connection));
			if(batchSize < 1)
				throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки должен быть положительным");

			var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
			var wanted = MySqlMultiBase.Distinct(databases);
			if(wanted.Count == 0)
				return result;

			var nameFilter = names?.ToList();
			// в SQL нет «прочитай таблицу, если она есть»: ветка UNION по отсутствующей таблице
			// уронила бы весь запрос, поэтому сначала спрашиваем, где она вообще есть
			var withTable = MySqlMultiBase.TableColumns(connection, wanted, ParametersTable).Keys.ToList();

			for(int offset = 0; offset < withTable.Count; offset += batchSize) {
				var batch = withTable.Skip(offset).Take(batchSize).ToList();
				try {
					ReadBatch(connection, batch, nameFilter, result);
				}
				catch(DbException ex) {
					// одна нечитаемая база не должна прятать остальные - дочитываем пачку по одной,
					// но тем же соединением: новых пулов при этом не появляется
					logger.Debug(ex, "Пакетное чтение {0} не удалось, читаем базы по одной", ParametersTable);
					foreach(var database in batch)
						ReadOne(connection, database, nameFilter, result);
				}
			}

			return result;
		}

		private static void ReadBatch(IDbConnection connection, IEnumerable<string> databases,
			IList<string> names, IDictionary<string, Dictionary<string, string>> result) {
			var parameters = new DynamicParameters();
			var projections = databases.Select(db => new KeyValuePair<string, string>(db, Projection));
			string union = MySqlMultiBase.UnionAll(projections, ParametersTable, BaseNameColumn, null, parameters);

			// имена фильтруем снаружи, одним условием на весь запрос, а не по разу в каждой ветке
			string sql = $"SELECT * FROM ({union}) p";
			if(names != null && names.Count > 0) {
				sql += " WHERE p.Name IN @names";
				parameters.Add("names", names);
			}

			foreach(var row in connection.Query<ParameterRow>(sql, parameters)) {
				if(!result.TryGetValue(row.BaseName, out var byName)) {
					byName = new Dictionary<string, string>();
					result[row.BaseName] = byName;
				}
				byName[row.Name] = row.StrValue;
			}
		}

		private static void ReadOne(IDbConnection connection, string database,
			IList<string> names, IDictionary<string, Dictionary<string, string>> result) {
			try {
				ReadBatch(connection, new[] { database }, names, result);
			}
			catch(DbException ex) {
				logger.Debug(ex, "Не удалось прочитать {0} в базе {1}", ParametersTable, database);
			}
		}

		/// <summary>Строка base_parameters вместе с базой, из которой она приехала</summary>
		private sealed class ParameterRow {
			public string BaseName { get; set; }
			public string Name { get; set; }
			public string StrValue { get; set; }
		}
	}
}
