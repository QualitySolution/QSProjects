using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace QS.BaseParameters
{
	public class ParametersService : DynamicObject {
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		#region Управление соединением

		private readonly DbConnection externalConnection;
		private DbConnection ownedConnection;
		private readonly Func<DbConnection> connectionFactory;
		
		private DbConnection connection {
			get {
				if(externalConnection != null)
					return externalConnection;
				if(ownedConnection == null)
					ownedConnection = connectionFactory();
				return ownedConnection;
			}
		}
		
		private void CloseConnectionIfNeeded()
		{
			if(externalConnection != null)
				return;
			if(ownedConnection != null) {
				ownedConnection.Close();
				ownedConnection = null;
			}
		}
		#endregion
		
		/// <summary>
		/// Данный конструктор получает внешнее соединение с базой, которым не управляет.
		/// Соединение с базой должен открывать и закрывать внешний владелец.
		/// </summary>
		/// <param name="externalConnection"></param>
		public ParametersService (DbConnection externalConnection) {
			this.externalConnection = externalConnection;
		}

		/// <summary>
		/// Конструктор получает фабрику для создания соединения с базой.
		/// Сервис при необходимости создает новое соединение через фабрику, сервис рассчитывает что соединение при создании будет уже открыто.
		/// Сервис не держит соединение открытым, а открывает и закрывает его при необходимости.
		/// </summary>
		public ParametersService (Func<DbConnection> connectionFactory)
		{
			this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
		}

		/// <summary>
		/// Используется только для создания класса в тестах не связанного с базой.
		/// </summary>
		protected ParametersService()
		{
		}

		private Dictionary<string, string> all;
		public Dictionary<string, string> All {
			get {
				if (all == null) {
					all = new Dictionary<string, string>();
					if(externalConnection != null || connectionFactory != null)
						ReloadParameters();
				}
				return all;
			}
		}
		
		#region Загрузка

		public void ReloadParameters()
		{
			all = new Dictionary<string, string>();
			string sql = "SELECT * FROM base_parameters";
			DbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = cmd.ExecuteReader()) {
				while (rdr.Read()) {
					all.Add(rdr["name"].ToString(), rdr["str_value"].ToString());
				}
			}
			CloseConnectionIfNeeded();
		}
		
		public async Task ReloadParametersAsync()
		{
			all = new Dictionary<string, string>();
			string sql = "SELECT * FROM base_parameters";
			DbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = await cmd.ExecuteReaderAsync()) {
				while (await rdr.ReadAsync()) {
					all.Add(rdr["name"].ToString(), rdr["str_value"].ToString());
				}
			}
			CloseConnectionIfNeeded();
		}

		#endregion

		#region Изменение параметров
		public void UpdateParameter (string name, object value)
		{
			string sql;
			if (All.ContainsKey (name))
			{
				if(All[name] == ConvertToString(value))
					return;

				sql = "UPDATE base_parameters SET str_value = @str_value WHERE name = @name";
			}
			else
				sql = "INSERT INTO base_parameters (name, str_value) VALUES (@name, @str_value)";

			logger.Debug ("Изменяем параметр базы {0}={1}", name, value);
			DbCommand cmd = connection.CreateCommand ();
			cmd.CommandText = sql;
			DbParameter paramName = cmd.CreateParameter ();
			paramName.ParameterName = "@name";
			paramName.Value = name;
			cmd.Parameters.Add (paramName);
			DbParameter paramValue = cmd.CreateParameter ();
			paramValue.ParameterName = "@str_value";
			paramValue.Value = ConvertToString(value);
			cmd.Parameters.Add (paramValue);
			cmd.ExecuteNonQuery ();
			CloseConnectionIfNeeded();

			All [name] = ConvertToString(value);
			logger.Debug ("Ок");
		}

		public void RemoveParameter (string name)
		{
			string sql;
			logger.Debug ("Удаляем параметр базы {0}", name);
			if (All.ContainsKey (name))
				sql = "DELETE FROM base_parameters WHERE name = @name";
			else
				throw new ArgumentException ("Нет указанного параметра базы", name);
			try {
				DbCommand cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				DbParameter paramName = cmd.CreateParameter();
				paramName.ParameterName = "@name";
				paramName.Value = name;
				cmd.Parameters.Add(paramName);
				cmd.ExecuteNonQuery();

				All.Remove(name);
				logger.Debug("Ок");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка удаления параметра");
				throw ex;
			}
			finally {
				CloseConnectionIfNeeded();
			}
		}
		#endregion

		#region Dynamic
		public dynamic Dynamic => this;

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if(indexes.Length == 1 && indexes[0] is string name) {
				if(All.TryGetValue(name, out string strValue)) {
					result = strValue;
				}
				else
					result = null;
				return true;
			}

			return base.TryGetIndex(binder, indexes, out result);
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			if(indexes.Length == 1 && indexes[0] is string name) {
				if(value == null)
					RemoveParameter(name);
				else
					UpdateParameter(name, value);
				return true;
			}

			return base.TrySetIndex(binder, indexes, value);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (All.TryGetValue(binder.Name, out string strValue)) {
				result = strValue;
			}
			else
				result = null;
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (value == null)
				RemoveParameter(binder.Name);
			else
				UpdateParameter(binder.Name, value);
			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			var returnType = args.FirstOrDefault() as Type;
			if (returnType == null)
				throw new NotSupportedException("Поддерживается только при вызове вида ParameterName(Type returnType).");

			if (All.TryGetValue(binder.Name, out string strValue))
				result = ConvertTo(strValue, returnType);
			else
				result = null;
			return true;
		}

		#endregion

		#region private

		private object ConvertTo(string value, Type type)
		{
			if (type == typeof(string)) {
				return value;
			}
			if (type == typeof(Version))
				return Version.Parse(value);

			TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
			if (typeConverter == null)
				throw new NotSupportedException($"Конвертация значения в {type} не поддерживается.");

			return typeConverter.ConvertFromInvariantString(value);
		}
		
		private string ConvertToString(object value)
		{
			if (value is string) {
				return (string)value;
			}

			TypeConverter typeConverter = TypeDescriptor.GetConverter(value.GetType());
			return typeConverter.ConvertToInvariantString(value);
		}

		#endregion
	}
}
