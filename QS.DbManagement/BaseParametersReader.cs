using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace QS.BaseParameters
{
	/// <summary> Чтение base_parameters сразу из многих баз одного сервера по одному соединению </summary>
	public static class BaseParametersReader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private const string ParametersTable = "base_parameters";
		private const int DefaultBatchSize = 50;

		/// <summary>
		/// Параметры перечисленных баз. Базы без таблицы base_parameters и базы, которые прочитать не удалось, в результат не попадают
		/// </summary>
		/// <param name="names">какие параметры нужны, null - все</param>
		/// <returns>base_name -> (name -> str_value)</returns>
		public static Dictionary<string, Dictionary<string, string>> ReadMany(
			DbConnection connection, IEnumerable<string> databases,
			IEnumerable<string> names = null, int batchSize = DefaultBatchSize)
		{
			if(connection == null)
				throw new ArgumentNullException(nameof(connection));
			if(databases == null)
				throw new ArgumentNullException(nameof(databases));
			if(batchSize < 1)
				throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки должен быть положительным");

			var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

			var wanted = databases
				.Where(db => !string.IsNullOrEmpty(db))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
			if(wanted.Count == 0)
				return result;

			var nameFilter = names?.ToList();

			var withTable = DatabasesWithParameters(connection, wanted);

			for(int offset = 0; offset < withTable.Count; offset += batchSize) {
				var batch = withTable.Skip(offset).Take(batchSize).ToList();
				try {
					ReadBatch(connection, batch, nameFilter, result);
				}
				catch(DbException ex) {
					// одна нечитаемая база не должна прятать остальные - дочитываем пачку по одной
					logger.Debug(ex, "Пакетное чтение {0} не удалось, читаем базы по одной", ParametersTable);
					foreach(var database in batch)
						ReadOne(connection, database, nameFilter, result);
				}
			}

			return result;
		}

		private static List<string> DatabasesWithParameters(DbConnection connection, IList<string> databases)
		{
			using(var command = connection.CreateCommand()) {
				var placeholders = new List<string>(databases.Count);
				for(int i = 0; i < databases.Count; i++) {
					placeholders.Add("@db" + i.ToString());
					AddParameter(command, "@db" + i.ToString(), databases[i]);
				}
				AddParameter(command, "@table", ParametersTable);

				SetCommandText(command, "SELECT table_schema FROM information_schema.tables "
					+ "WHERE table_name = @table AND table_schema IN (" + string.Join(", ", placeholders) + ")");

				var found = new List<string>();
				using(var reader = command.ExecuteReader()) {
					while(reader.Read())
						found.Add(reader.GetString(0));
				}
				return found;
			}
		}

		private static void ReadBatch(DbConnection connection, IList<string> databases,
			IList<string> names, IDictionary<string, Dictionary<string, string>> result)
		{
			using(var command = connection.CreateCommand()) {
				var selects = new List<string>(databases.Count);
				for(int i = 0; i < databases.Count; i++) {
					// имя базы уходит в запрос дважды: колонкой-меткой - параметром, идентификатором - в кавычках
					AddParameter(command, "@d" + i.ToString(), databases[i]);
					selects.Add($"SELECT @d{i} AS base_name, name, str_value FROM `{Identifier(databases[i])}`.{ParametersTable}");
				}

				var sql = new StringBuilder("SELECT p.base_name, p.name, p.str_value FROM (")
					.Append(string.Join(" UNION ALL ", selects))
					.Append(") p");
				AppendNameFilter(sql, command, names);
				SetCommandText(command, sql.ToString());

				Fill(command, result);
			}
		}

		private static void ReadOne(DbConnection connection, string database,
			IList<string> names, IDictionary<string, Dictionary<string, string>> result)
		{
			try {
				using(var command = connection.CreateCommand()) {
					AddParameter(command, "@d0", database);

					var sql = new StringBuilder(
						$"SELECT @d0 AS base_name, name, str_value FROM `{Identifier(database)}`.{ParametersTable} p");
					AppendNameFilter(sql, command, names);
					SetCommandText(command, sql.ToString());

					Fill(command, result);
				}
			}
			catch(DbException ex) {
				logger.Debug(ex, "Не удалось прочитать {0} в базе {1}", ParametersTable, database);
			}
		}

		private static void AppendNameFilter(StringBuilder sql, DbCommand command, IList<string> names)
		{
			if(names == null || names.Count == 0)
				return;

			var placeholders = new List<string>(names.Count);
			for(int i = 0; i < names.Count; i++) {
				placeholders.Add("@n" + i.ToString());
				AddParameter(command, "@n" + i.ToString(), names[i]);
			}
			sql.Append(" WHERE p.name IN (").Append(string.Join(", ", placeholders)).Append(")");
		}

		private static void Fill(DbCommand command, IDictionary<string, Dictionary<string, string>> result)
		{
			using(var reader = command.ExecuteReader()) {
				while(reader.Read()) {
					string database = reader["base_name"].ToString();
					if(!result.TryGetValue(database, out var parameters)) {
						parameters = new Dictionary<string, string>();
						result[database] = parameters;
					}
					parameters[reader["name"].ToString()] = reader["str_value"].ToString();
				}
			}
		}

		[SuppressMessage("", "CA2100", Justification = "Идентификатор экранируется, значения передаются параметрами")]
		private static void SetCommandText(DbCommand command, string sql) => command.CommandText = sql;

		private static void AddParameter(DbCommand command, string name, object value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = DbType.String;
			parameter.Value = value;
			command.Parameters.Add(parameter);
		}

		private static string Identifier(string value) => value.Replace("`", "``");
	}
}
