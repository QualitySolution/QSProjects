using MySqlConnector;
using System;
using System.Globalization;
using System.Text;

namespace QS.Project.DB {
	public static class DatabaseExtensions {
		public static string GetConnectionString(this MySqlConnectionStringBuilder stringBuilder, bool includePass) {
			if(includePass) {
				return stringBuilder.ConnectionString;
			}

			StringBuilder resultBuilder = new StringBuilder();
			string text = "";
			foreach(string key in stringBuilder.Keys) {
				if(string.Compare(key, "password", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(key, "pwd", StringComparison.OrdinalIgnoreCase) != 0) {
					resultBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}{1}={2}", new object[3]
					{
						text,
						key,
						stringBuilder[key]
					});
					text = ";";
				}
			}

			return resultBuilder.ToString();
		}
	}
}
