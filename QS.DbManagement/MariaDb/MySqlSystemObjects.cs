using System.Collections.Generic;

namespace QS.DbManagement.MariaDb {
	internal static class MySqlSystemObjects {
		public static readonly IReadOnlyCollection<string> Databases =
			new[] { "information_schema", "mysql", "performance_schema", "sys" };

		public static readonly IReadOnlyCollection<string> Users =
			new[] { "root", "mariadb.sys", "mysql", "PUBLIC" };
	}
}
