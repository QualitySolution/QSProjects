using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QS.DbManagement.MariaDb {
	/// <summary>
	/// Разбор строк, которые отдаёт SHOW GRANTS
	/// </summary>
	internal static class MySqlGrants {
		private const string AllPrivileges = "ALL PRIVILEGES";
		private const string GrantOption = "WITH GRANT OPTION";

		// Скобки после привилегии
		private static readonly Regex ColumnList =
			new Regex(@"\([^)]*\)", RegexOptions.None, TimeSpan.FromSeconds(5));

		public static bool HasGlobalAdmin(IEnumerable<string> grants)
			=> grants.Any(g => Scope(g) == "*" && Privileges(g).Contains(AllPrivileges));

		public static bool IsGlobalAdmin(bool superPriv, bool createUserPriv)
			=> superPriv && createUserPriv;

		public static bool HasGlobalGrantOption(IEnumerable<string> grants) =>
			grants.Any(g => Scope(g) == "*" && HasGrantOption(g));

		public static bool HasGrantOption(string grant) =>
			grant.IndexOf(GrantOption, StringComparison.OrdinalIgnoreCase) >= 0;

		public static string Scope(string grant) {
			int on = grant.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
			int to = grant.IndexOf(" TO ", StringComparison.OrdinalIgnoreCase);
			if(on < 0 || to < on)
				return null;

			string target = grant.Substring(on + 4, to - on - 4).Trim(); //4 = длина " ON "
			if(!target.EndsWith(".*", StringComparison.Ordinal))
				return null;

			string name = target.Substring(0, target.Length - 2).Trim();
			if(name == "*")
				return name;
			if(name.Length < 2 || !name.StartsWith("`", StringComparison.Ordinal) || !name.EndsWith("`", StringComparison.Ordinal))
				return null;
			return name
				.Substring(1, name.Length - 2)
				.Replace("``", "`");
		}

		public static IEnumerable<string> Privileges(string grant) {
			int start = grant.IndexOf("GRANT ", StringComparison.OrdinalIgnoreCase);
			int on = grant.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
			if(start < 0 || on <= start)
				return Enumerable.Empty<string>();

			start += 6; //длина "GRANT "
			return ColumnList
				.Replace(
					grant.Substring(start, on - start), string.Empty)
				.Split(',')
				.Select(p =>
					p.Trim()
				.ToUpperInvariant())
				.Where(p =>
					p.Length > 0);
		}

		public static bool IsMeaningful(string grant)
		{
			return Privileges(grant)
				.Any(p => p != "USAGE");
		}
	}
}
