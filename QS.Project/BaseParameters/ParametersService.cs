using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using NLog;

namespace QS.BaseParameters
{
	public class ParametersService : DynamicObject
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public Dictionary<string, object> All = new Dictionary<string, object>();
		private readonly DbConnection connection;

		public ParametersService (DbConnection connection)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			ReloadParameters();
		}

		#region Загрузка

		public void ReloadParameters()
		{
			All.Clear();
			string sql = "SELECT * FROM base_parameters";
			DbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = cmd.ExecuteReader()) {
				while (rdr.Read()) {
					All.Add(rdr["name"].ToString(), rdr["str_value"].ToString());
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
				if(All[name] == value)
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
			paramValue.Value = value;
			cmd.Parameters.Add (paramValue);
			cmd.ExecuteNonQuery ();

			All [name] = value;
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
				DbCommand cmd = connection.CreateCommand ();
				cmd.CommandText = sql;
				DbParameter paramName = cmd.CreateParameter ();
				paramName.ParameterName = "@name";
				paramName.Value = name;
				cmd.Parameters.Add (paramName);
				cmd.ExecuteNonQuery ();

				All.Remove (name);
				logger.Debug ("Ок");
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка удаления параметра");
				throw ex;
			}
		}
		#endregion

		#region Dynamic

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return All.TryGetValue(binder.Name, out result);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (value == null)
				RemoveParameter(binder.Name);
			else
				UpdateParameter(binder.Name, value);
			return true;
		} 

		#endregion
	}
}