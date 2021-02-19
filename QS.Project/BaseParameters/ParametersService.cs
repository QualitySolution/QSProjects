using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using NLog;
using QS.Project.DB;

namespace QS.BaseParameters
{
	public class ParametersService : DynamicObject
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public Dictionary<string, string> All = new Dictionary<string, string>();
		private readonly IConnectionFactory connectionFactory;

		public ParametersService (IConnectionFactory connectionFactory)
		{
			this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			ReloadParameters();
		}

		/// <summary>
		/// Используется только для создания класса в тестах не связанного с базой.
		/// </summary>
		protected ParametersService()
		{
		}

		#region Загрузка

		public void ReloadParameters()
		{
			All.Clear();
			string sql = "SELECT * FROM base_parameters";
			using(var connection = connectionFactory.OpenConnection()) {
				DbCommand cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				using(DbDataReader rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						All.Add(rdr["name"].ToString(), rdr["str_value"].ToString());
					}
				}
			}
		}

		#endregion

		#region Изменение параметров
		public void UpdateParameter (string name, object value)
		{
			string sql;
			if (All.ContainsKey (name))
			{
				if(All[name] == value.ToString())
					return;

				sql = "UPDATE base_parameters SET str_value = @str_value WHERE name = @name";
			}
			else
				sql = "INSERT INTO base_parameters (name, str_value) VALUES (@name, @str_value)";

			logger.Debug ("Изменяем параметр базы {0}={1}", name, value);
			using(var connection = connectionFactory.OpenConnection()) {
				DbCommand cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				DbParameter paramName = cmd.CreateParameter();
				paramName.ParameterName = "@name";
				paramName.Value = name;
				cmd.Parameters.Add(paramName);
				DbParameter paramValue = cmd.CreateParameter();
				paramValue.ParameterName = "@str_value";
				paramValue.Value = value.ToString();
				cmd.Parameters.Add(paramValue);
				cmd.ExecuteNonQuery();
			}
			All [name] = value.ToString();
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
				using(var connection = connectionFactory.OpenConnection()) {
					DbCommand cmd = connection.CreateCommand();
					cmd.CommandText = sql;
					DbParameter paramName = cmd.CreateParameter();
					paramName.ParameterName = "@name";
					paramName.Value = name;
					cmd.Parameters.Add(paramName);
					cmd.ExecuteNonQuery();
				}
				All.Remove (name);
				logger.Debug ("Ок");
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка удаления параметра");
				throw ex;
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
				throw new NotSupportedException($"Конвертация значения в {type} не поддеживается.");

			return typeConverter.ConvertFromString(value);
		}

		#endregion
	}
}