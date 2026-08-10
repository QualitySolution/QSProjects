using MySqlConnector;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement.MariaDb {
	/// <summary>Перевод между грантами сервера и правами доступа приложения</summary>
	internal static class MySqlAccess {
		public const string AllPrivileges = "ALL PRIVILEGES";
		private const string ReadOnlyPrivileges = "SELECT, LOCK TABLES, SHOW VIEW";
		private const string EditPrivileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, CREATE TEMPORARY TABLES, LOCK TABLES, SHOW VIEW";

		/// <summary>переводит в вид в виде 'логин'@'хост'</summary>
		public static string UserOf(string login, string host)
			=> $"'{MySqlHelper.EscapeString(login)}'@'{MySqlHelper.EscapeString(host)}'";

		/// <summary>
		/// Выдача глобального админа: права на весь сервер плюс право их раздавать.
		/// </summary>
		public static string GrantAdmin(string account)
			=> $"GRANT {AllPrivileges} ON *.* TO {account} WITH GRANT OPTION";

		/// <summary>Снятие глобального админа</summary>
		public static string RevokeAdmin(string account)
			=> $"REVOKE {AllPrivileges} ON *.* FROM {account}; REVOKE GRANT OPTION ON *.* FROM {account}";

		public static DbUserBaseAccess FullAccessByGlobalGrant(DbInfo db)
			=> new DbUserBaseAccess {
				BaseName = db.BaseName,
				Title = db.Title,
				HasAccess = true,
				IsAdmin = true,
				CanEdit = false
			};

		public static DbUserBaseAccess FromGrants(DbInfo db, IEnumerable<string> grants)
		{
			var access = new DbUserBaseAccess { BaseName = db.BaseName, Title = db.Title };

			var privileges = grants
				.Where(g => CoversDatabase(g, db.BaseName) && MySqlGrants.IsMeaningful(g))
				.SelectMany(MySqlGrants.Privileges)
				.ToList();
			if(!privileges.Any())
				return access;

			access.HasAccess = true;
			if(privileges.Contains(AllPrivileges))
				access.IsAdmin = true;
			else if(privileges.All(IsReadOnlyPrivilege))
				access.ReadOnly = true;
			return access;
		}

		/// <summary>
		/// SQL, чтобы у <paramref name="login"/>@<paramref name="host"/> стал доступ <paramref name="access"/>
		/// </summary>
		public static IEnumerable<string> Statements(string login, string host, IEnumerable<string> grants, DbUserBaseAccess access)
		{
			string user = UserOf(login, host);

			foreach(var grant in (grants ?? Enumerable.Empty<string>()).Where(g => GrantedOnDatabase(g, access.BaseName))) {
				string pattern = $"`{MySqlEscape.Identifier(MySqlGrants.Scope(grant))}`.*";
				if(MySqlGrants.IsMeaningful(grant))
					yield return $"REVOKE {AllPrivileges} ON {pattern} FROM {user}";
				// ALL PRIVILEGES не включает право раздачи грантов
				if(MySqlGrants.HasGrantOption(grant))
					yield return $"REVOKE GRANT OPTION ON {pattern} FROM {user}";
			}

			string privileges = PrivilegesFor(access);
			if(privileges != null)
				yield return $"GRANT {privileges} ON `{MySqlEscape.Pattern(access.BaseName)}`.* TO {user}";
		}

		/// <summary>null - доступа нет</summary>
		private static string PrivilegesFor(DbUserBaseAccess access)
		{
			if(!access.HasAccess)
				return null;
			if(access.IsAdmin)
				return AllPrivileges;
			if(access.ReadOnly)
				return ReadOnlyPrivileges;
			return EditPrivileges;
		}

		// грант на всём сервере *.* базу тоже покрывает
		private static bool CoversDatabase(string grant, string baseName)
		{
			string scope = MySqlGrants.Scope(grant);
			if(scope == null)
				return false;
			return scope == "*" || string.Equals(MySqlEscape.UnescapePattern(scope), baseName, StringComparison.OrdinalIgnoreCase);
		}

		// отзывать по имени базы можно только грант, выданный именно на неё
		private static bool GrantedOnDatabase(string grant, string baseName)
		{
			string scope = MySqlGrants.Scope(grant);
			return scope != null && scope != "*"
				&& string.Equals(MySqlEscape.UnescapePattern(scope), baseName, StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsReadOnlyPrivilege(string privilege)
			=> privilege == "SELECT" || privilege == "LOCK TABLES" || privilege == "SHOW VIEW";
	}
}
