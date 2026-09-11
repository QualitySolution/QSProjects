namespace QS.DbManagement.MariaDb {
	internal static class MySqlEscape {
		/// <summary>идентификатор в обратных кавычках</summary>
		public static string Identifier(string value)
		{
			return value == null ? string.Empty : value.Replace("`", "``");
		}

		/// <summary>имя базы для GRANT</summary>
		public static string Pattern(string dbName)
		{
			return Identifier(dbName).Replace("_", "\\_").Replace("%", "\\%");
		}

		/// <summary>имя базы, как его вернул SHOW GRANTS</summary>
		public static string UnescapePattern(string pattern)
		{
			return pattern.Replace("\\_", "_").Replace("\\%", "%");
		}
	}
}
