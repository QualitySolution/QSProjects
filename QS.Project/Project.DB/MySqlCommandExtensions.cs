using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace QS.Project.DB
{
	public static class MySqlCommandExtensions
	{
		/// <summary>
		/// This will add an array of parameters to a MySqlCommand. This is used for an IN statement.
		/// Use the returned value for the IN part of your MySql call. (i.e. SELECT * FROM table WHERE field IN ({paramNameRoot}))
		/// </summary>
		/// <param name="mySqlCommand">The MySqlCommand object to add parameters to.</param>
		/// <param name="paramNameRoot">What the parameter should be named followed by a unique value for each value. This value will be replaced in the CommandText.</param>
		/// <param name="values">The array of strings that need to be added as parameters.</param>
		/// <param name="dbType">One of the System.Data.MySqlDbType values. If null, determines type based on T.</param>
		/// <param name="size">The maximum size, in bytes, of the data within the column. The default value is inferred from the parameter value.</param>
		public static MySqlParameter[] AddArrayParameters<T>(this MySqlCommand mySqlCommand, string paramNameRoot, IEnumerable<T> values,
			MySqlDbType? dbType = null, int? size = null)
		{
			// An array cannot be simply added as a parameter to a MySqlCommand so we need to loop through things and add it manually.
			// Each item in the array will end up being it's own MySqlParameter so the return value for this must be used as part of the
			// IN statement in the CommandText.
			var parameters = new List<MySqlParameter>();
			var parameterNames = new List<string>();
			var paramNumber = 1;
			foreach(var value in values)
			{
				var paramName = $"{paramNameRoot}{paramNumber++}";
				parameterNames.Add(paramName);
				MySqlParameter p = new MySqlParameter(paramName, value);
				if(dbType.HasValue)
					p.MySqlDbType = dbType.Value;
				if(size.HasValue)
					p.Size = size.Value;
				mySqlCommand.Parameters.Add(p);
				parameters.Add(p);
			}

			mySqlCommand.CommandText = mySqlCommand.CommandText.Replace(paramNameRoot, string.Join(",", parameterNames));

			return parameters.ToArray();
		}
	}
}
