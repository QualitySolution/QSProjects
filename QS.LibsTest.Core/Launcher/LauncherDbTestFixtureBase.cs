using Dapper;
using MySqlConnector;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Testing.DB;
using Testcontainers.MariaDb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test {
	/// <summary>
	/// Фикстуры идут по очереди и делят один сервер - см. <see cref="TestMariaDbServer"/>.
	/// Параллельными им быть нельзя: <see cref="ResetServer"/> сносит на сервере все базы
	/// и все учётки, кроме системных, а имя метабазы - константа, своей копии не сделать
	/// </summary>
	public abstract class LauncherDbTestFixtureBase : MariaDbTestContainerSqlFixtureBase {
		protected const string LauncherDbName = "QSLauncher";
		protected const string RootLogin = "root";
		protected const string RootPassword = "root";

		/// <summary>Код целевого продукта</summary>
		protected const byte TestProductCode = 42;

		/// <summary>Код чужого продукта - его базы наш лаунчер видеть не должен</summary>
		protected const byte OtherProductCode = 77;

		/// <summary>Базы, которые ResetServer никогда не трогает</summary>
		private static readonly string[] KeptDatabases =
			{ "information_schema", "mysql", "performance_schema", "sys", LauncherDbName };

		/// <summary>Учётки сервера, которые ResetServer никогда не трогает</summary>
		private static readonly string[] KeptLogins = { "root", "mariadb.sys", "mysql", "PUBLIC", "" };

		private readonly List<IDisposable> createdProviders = new List<IDisposable>();
		private Stopwatch testTimer;

		#region Жизненный цикл

		[OneTimeSetUp]
		public override async Task OneTimeSetUp() {
			ConfigureLogging();
			MariaDbContainer = await TestMariaDbServer.GetAsync();
			await DeployMetabase();
		}

		/// <summary>Сервер общий на всю сборку - гасит его <see cref="TestMariaDbServer"/></summary>
		[OneTimeTearDown]
		public override Task OneTimeTearDown() => Task.CompletedTask;

		private static void ConfigureLogging() {
			if(NLog.LogManager.Configuration != null)
				return;

			var configuration = new NLog.Config.LoggingConfiguration();
			var target = new TestProgressTarget {
				Layout = "  ${level:uppercase=true:padding=-5} ${logger:shortName=true} | ${message}${onexception: -> ${exception:format=Message}}"
			};
			configuration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, target, "QS.*");
			configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, target, "*");
			NLog.LogManager.Configuration = configuration;
		}

		/// <summary>Цель NLog, отдающая записи в лог текущего теста</summary>
		private sealed class TestProgressTarget : NLog.Targets.TargetWithLayout {
			protected override void Write(NLog.LogEventInfo logEvent) {
				Log(RenderLogEvent(Layout, logEvent));
			}
		}

		private static void Log(string line) {
			TestContext.Out.WriteLine(line);
		}

		protected static void LogStep(string format, params object[] args) {
			Log("  · " + (args.Length == 0 ? format : string.Format(format, args)));
		}

		[SetUp]
		public virtual async Task ResetServer() {
			testTimer = Stopwatch.StartNew();
			var test = TestContext.CurrentContext.Test;
			Log($"┌─ {test.ClassName?.Split('.').Last()}.{test.Name}");
			if(test.Properties.Get("Description") is string description)
				Log($"│  {description}");

			await DropApplicationDatabases();
			await DropTestLogins();
			await TruncateMetabase();
			await SeedMetabaseUser(RootLogin, isAdmin: true);
			LogStep("сервер приведён в исходное: в метабазе только {0}", RootLogin);
		}

		[TearDown]
		public virtual async Task FinishTest() {
			foreach(var provider in createdProviders)
				provider.Dispose();
			createdProviders.Clear();

			// Dispose провайдера возвращает соединение в пул, а не закрывает его на сервере.
			// Учёток за прогон десятки, у каждой свой пул - на общем сервере это упирается
			// в max_connections. Пулы живут в процессе тестов, поэтому чистим их сами
			await MySqlConnection.ClearAllPoolsAsync();

			var result = TestContext.CurrentContext.Result;
			if(result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
				await DumpServerState();

			TestContext.Progress.WriteLine(
				$"└─ {result.Outcome.Status}, {testTimer?.ElapsedMilliseconds ?? 0} мс\n");
		}

		/// <summary>
		/// Снимок сервера на момент падения
		/// </summary>
		private async Task DumpServerState() {
			Log("│  СОСТОЯНИЕ СЕРВЕРА НА МОМЕНТ ПАДЕНИЯ:");
			try {
				Log($"│    базы на сервере: {Describe(await ListApplicationDatabases())}");

				if(await DatabaseExists(LauncherDbName)) {
					foreach(var row in await ReadMetabaseBases())
						Log($"│    метабаза/bases: id={row.Id} {row.BaseName} «{row.Title}» v{row.Version} disabled={row.Disabled}");
					foreach(var row in await ReadMetabaseUsers())
						Log($"│    метабаза/server_users: id={row.Id} {row.Login} «{row.Name}» admin={row.IsAdmin} disabled={row.Disabled}");
					foreach(var row in await ReadBaseUpdateRights())
						Log($"│    метабаза/base_update_rights: {row.Login} -> {row.BaseName} canUpdate={row.CanUpdate}");
				}
				else
					Log("│    метабазы нет");

				using(var connection = CreateConnection(withoutDb: true)) {
					await connection.OpenAsync();
					var logins = (await connection.QueryAsync<string>(
						"SELECT DISTINCT User FROM mysql.user WHERE User NOT IN @kept AND User NOT LIKE 'mysql.%' ORDER BY User;",
						new { kept = KeptLogins })).ToList();
					Log($"│    учётки сервера: {Describe(logins)}");
				}
			}
			catch(Exception ex) {
				Log($"│    снять состояние не удалось: {ex.Message}");
			}
		}

		private static string Describe(ICollection<string> items) =>
			items.Count == 0 ? "нет" : string.Join(", ", items);

		private static async Task<string> ReadScript(string fileName) {
			string resource = ScriptResourcePrefix + fileName;
			using(var stream = typeof(LauncherDbTestFixtureBase).Assembly.GetManifestResourceStream(resource)) {
				if(stream == null)
					throw new InvalidOperationException($"В сборке нет встроенного ресурса {resource}");

				using(var reader = new StreamReader(stream))
					return await reader.ReadToEndAsync();
			}
		}

		private const string ScriptResourcePrefix = "QS.Launcher.Test.Base.";

		#endregion

		#region Метабаза

		protected async Task DeployMetabase() {
			var script = await ReadScript("QSLauncher.sql");
			await PrepareDatabase(script, dbName: LauncherDbName);
		}

		/// <summary>Сносит метабазу целиком - для проверки работы без неё.</summary>
		protected async Task DropMetabase() {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{LauncherDbName}`;");
			}
			LogStep("метабаза {0} снесена", LauncherDbName);
		}

		private async Task TruncateMetabase() {
			if(!await DatabaseExists(LauncherDbName)) {
				await DeployMetabase();
				return;
			}

			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync(
					"DELETE FROM `base_update_rights`; DELETE FROM `bases`; DELETE FROM `server_users`;");
			}
		}

		protected async Task<int> SeedMetabaseUser(string login, bool isAdmin = false,
			string name = null, string email = null, bool disabled = false, byte product = TestProductCode) {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync(
					"INSERT INTO `server_users` (product_id, login, name, email, is_admin, disabled) " +
					"VALUES (@product, @login, @name, @email, @admin, @disabled);",
					new { product, login, name, email, admin = isAdmin, disabled });
				int id = await connection.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID();");
				LogStep("в метабазу добавлен пользователь {0} (id {1}, управляет пользователями: {2})", login, id, isAdmin);
				return id;
			}
		}

		protected async Task<int> SeedMetabaseBase(string baseName, string title = null,
			byte product = TestProductCode, string version = "1.0", bool disabled = false) {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync(
					"INSERT INTO `bases` (product_id, base_name, base_title, version, disabled) " +
					"VALUES (@product, @name, @title, @version, @disabled);",
					new { product, name = baseName, title = title ?? baseName, version, disabled });
				return await connection.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID();");
			}
		}

		protected async Task GrantBaseUpdateRight(int userId, int baseId, bool canUpdate = true) {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync(
					"INSERT INTO `base_update_rights` (user_id, base_id, can_update) VALUES (@user, @base, @canUpdate) " +
					"ON DUPLICATE KEY UPDATE can_update = VALUES(can_update);",
					new { user = userId, @base = baseId, canUpdate });
			}
			LogStep("в метабазе отмечено право на обновление: пользователь {0} -> база {1} ({2})",
				userId, baseId, canUpdate);
		}

		#endregion

		#region Чтение метабазы для проверок

		protected async Task<List<MetabaseBaseRow>> ReadMetabaseBases() {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<MetabaseBaseRow>(
					"SELECT id AS Id, base_name AS BaseName, base_title AS Title, " +
					"version AS Version, disabled AS Disabled, product_id AS ProductId " +
					"FROM `bases` ORDER BY base_name;")).ToList();
			}
		}

		protected async Task<MetabaseBaseRow> ReadMetabaseBase(string baseName) =>
			(await ReadMetabaseBases()).FirstOrDefault(b =>
				string.Equals(b.BaseName, baseName, StringComparison.OrdinalIgnoreCase));

		protected async Task<List<MetabaseUserRow>> ReadMetabaseUsers() {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<MetabaseUserRow>(
					"SELECT id AS Id, login AS Login, name AS Name, email AS Email, phone AS Phone, " +
					"is_admin AS IsAdmin, disabled AS Disabled " +
					"FROM `server_users` ORDER BY login;")).ToList();
			}
		}

		protected async Task<MetabaseUserRow> ReadMetabaseUser(string login) =>
			(await ReadMetabaseUsers()).FirstOrDefault(u =>
				string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase));

		protected async Task<List<MetabaseUpdateRightRow>> ReadBaseUpdateRights() {
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<MetabaseUpdateRightRow>(
					"SELECT r.user_id AS UserId, r.base_id AS BaseId, r.can_update AS CanUpdate, " +
					"u.login AS Login, b.base_name AS BaseName " +
					"FROM `base_update_rights` r " +
					"LEFT JOIN `server_users` u ON u.id = r.user_id " +
					"LEFT JOIN `bases` b ON b.id = r.base_id;")).ToList();
			}
		}

		#endregion

		#region Прикладные базы

		protected async Task CreateApplicationDatabase(string dbName, string title = null,
			byte product = TestProductCode, string version = "1.0",
			bool withUsersTable = true, bool withDeactivatedColumn = true, bool withParameters = true) {
			var script = await ReadScript("AppBase.sql");
			await PrepareDatabase(script, dbName: dbName);

			using(var connection = CreateConnection(dbName)) {
				await connection.OpenAsync();

				if(withParameters)
					await WriteBaseParameters(connection, dbName, title, product, version);

				await AdjustUsersTable(connection, withUsersTable, withDeactivatedColumn);
			}

			LogStep("создана база {0} «{1}» (продукт {2}, версия {3}{4})",
				dbName, title ?? dbName, product, version,
				DescribeOmissions(withParameters, withUsersTable, withDeactivatedColumn));
		}

		// BaseGuid пишет сама база при наполнении, метабаза его не хранит - но параметр
		// в base_parameters реальный, и чтение метаинформации обязано его пережить
		private static Task WriteBaseParameters(MySqlConnection connection, string dbName,
			string title, byte product, string version) =>
			connection.ExecuteAsync(
				"INSERT INTO `base_parameters` (name, str_value) VALUES " +
				"('ProductCode', @product), ('BaseTitle', @title), ('version', @version), ('BaseGuid', @guid);",
				new {
					product = product.ToString(), title = title ?? dbName, version,
					guid = Guid.NewGuid().ToString()
				});

		private static async Task AdjustUsersTable(MySqlConnection connection, bool withUsersTable, bool withDeactivatedColumn) {
			if(!withUsersTable)
				await connection.ExecuteAsync("DROP TABLE `users`;");
			else if(!withDeactivatedColumn)
				await connection.ExecuteAsync("ALTER TABLE `users` DROP COLUMN `deactivated`;");
		}

		private static string DescribeOmissions(bool withParameters, bool withUsersTable, bool withDeactivatedColumn) {
			var omissions = new List<string>();
			if(!withParameters)
				omissions.Add("без base_parameters");
			if(!withUsersTable)
				omissions.Add("без таблицы users");
			else if(!withDeactivatedColumn)
				omissions.Add("без колонки deactivated");

			return omissions.Count == 0 ? string.Empty : ", " + string.Join(", ", omissions);
		}

		protected async Task CreateForeignDatabase(string dbName) {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{dbName}`; CREATE DATABASE `{dbName}`;");
			}
			using(var connection = CreateConnection(dbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("CREATE TABLE `something` (`id` INT PRIMARY KEY);");
			}
			LogStep("создана чужая база {0} - без base_parameters, продукту не принадлежит", dbName);
		}

		protected async Task<List<BaseUsersTableRow>> ReadBaseUsers(string dbName) {
			using(var connection = CreateConnection(dbName)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<BaseUsersTableRow>(
					"SELECT id AS Id, login AS Login, name AS Name, email AS Email, " +
					"admin AS Admin, deactivated AS Deactivated FROM `users` ORDER BY login;")).ToList();
			}
		}

		protected async Task<BaseUsersTableRow> ReadBaseUser(string dbName, string login) =>
			(await ReadBaseUsers(dbName)).FirstOrDefault(u =>
				string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase));

		protected async Task<bool> DatabaseExists(string dbName) {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				return await connection.ExecuteScalarAsync<int>(
					"SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name;",
					new { name = dbName }) > 0;
			}
		}

		private async Task<List<string>> ListApplicationDatabases() {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<string>("SHOW DATABASES"))
					.Where(db => !KeptDatabases.Contains(db, StringComparer.OrdinalIgnoreCase))
					.ToList();
			}
		}

		private async Task DropApplicationDatabases() {
			foreach(var dbName in await ListApplicationDatabases()) {
				using(var connection = CreateConnection(withoutDb: true)) {
					await connection.OpenAsync();
					await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{dbName}`;");
				}
			}
		}

		#endregion

		#region Учётки сервера

		/// <summary>Заводит учётку прямо на сервере, минуя лаунчер.</summary>
		protected async Task CreateServerLogin(string login, string password,
			bool isAdmin = false, string host = "%", bool locked = false) {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				string account = $"'{login}'@'{host}'";
				await connection.ExecuteAsync(
					$"CREATE USER {account} IDENTIFIED BY '{password}'{(locked ? " ACCOUNT LOCK" : string.Empty)};");
				if(isAdmin)
					await connection.ExecuteAsync($"GRANT ALL PRIVILEGES ON *.* TO {account} WITH GRANT OPTION;");
			}
			LogStep("на сервере заведена учётка {0}@{1}{2}{3}", login, host,
				isAdmin ? " с глобальными правами" : " без прав",
				locked ? ", заблокирована" : string.Empty);
		}

		protected async Task GrantOnDatabase(string login, string dbName, string privileges, string host = "%") {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync($"GRANT {privileges} ON `{dbName}`.* TO '{login}'@'{host}';");
			}
			LogStep("на сервере выданы права {0} на {1} пользователю {2}@{3}", privileges, dbName, login, host);
		}

		protected async Task<List<string>> ReadServerGrants(string login, string host = "%") {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<string>($"SHOW GRANTS FOR '{login}'@'{host}'")).ToList();
			}
		}

		protected async Task<List<string>> ReadServerLoginHosts(string login) {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				return (await connection.QueryAsync<string>(
					"SELECT Host FROM mysql.user WHERE User = @login ORDER BY Host;", new { login })).ToList();
			}
		}

		protected static bool GrantsMentionDatabase(IEnumerable<string> grants, string dbName) =>
			// Имя базы в GRANT экранируется как шаблон base\_name, поэтому SHOW GRANTS отдаёт его с обратными слешами
			grants.Any(g => Unescape(g).IndexOf($"`{dbName}`", StringComparison.OrdinalIgnoreCase) >= 0);

		/// <summary>Грант на указанную базу в человекочитаемом виде, null - его нет</summary>
		protected static string FindGrantOnDatabase(IEnumerable<string> grants, string dbName) =>
			grants.Select(Unescape)
				.FirstOrDefault(g => g.IndexOf($"`{dbName}`", StringComparison.OrdinalIgnoreCase) >= 0);

		private static string Unescape(string grant) =>
			grant.Replace("\\_", "_").Replace("\\%", "%");

		protected async Task<bool> ServerLoginExists(string login) =>
			(await ReadServerLoginHosts(login)).Count > 0;

		private async Task DropTestLogins() {
			using(var connection = CreateConnection(withoutDb: true)) {
				await connection.OpenAsync();
				var accounts = (await connection.QueryAsync<(string User, string Host)>(
					"SELECT User, Host FROM mysql.user;")).ToList();

				foreach(var account in accounts) {
					if(KeptLogins.Contains(account.User, StringComparer.OrdinalIgnoreCase)
						|| account.User.StartsWith("mysql.", StringComparison.OrdinalIgnoreCase))
						continue;
					await connection.ExecuteAsync($"DROP USER IF EXISTS '{account.User}'@'{account.Host}';");
				}
				await connection.ExecuteAsync("FLUSH PRIVILEGES;");
			}
		}

		#endregion

		#region Провайдер

		protected MariaDBProvider CreateProvider(string login = RootLogin, string password = RootPassword,
			byte productCode = TestProductCode) {
			var builder = GetConnectionStringBuilder(withoutDb: true);
			var parameters = new List<ConnectionParameterValue> {
				new ConnectionParameterValue(new ConnectionParameter("Server", "Сервер"), $"{builder.Server}:{builder.Port}"),
				new ConnectionParameterValue(new ConnectionParameter("Login", "Пользователь"), login)
			};

			var provider = new MariaDBProvider(parameters, productCode, password);
			createdProviders.Add(provider);
			LogStep("собран провайдер: {0}@{1}:{2}, продукт {3}", login, builder.Server, builder.Port, productCode);
			return provider;
		}

		/// <summary>Провайдер после успешного входа - состояние, из которого работают все страницы лаунчера.</summary>
		protected MariaDBProvider LoginAs(string login = RootLogin, string password = RootPassword,
			byte productCode = TestProductCode) {
			var provider = CreateProvider(login, password, productCode);
			var response = provider.LoginToServer();
			Assert.That(response.Success, Is.True, $"Не удалось войти на сервер как {login}: {response.ErrorMessage}");
			LogStep("вход выполнен как {0}: админ={1}, создание баз={2}, удаление баз={3}, управление пользователями={4}",
				login, provider.IsAdmin, provider.CanCreateDatabase, provider.CanDropDatabase, provider.CanManageUsers);
			return provider;
		}

		#endregion

		#region Строки для проверок

		protected class MetabaseBaseRow {
			public int Id { get; set; }
			public string BaseName { get; set; }
			public string Title { get; set; }
			public string Version { get; set; }
			public bool Disabled { get; set; }
			public byte ProductId { get; set; }
		}

		protected class MetabaseUserRow {
			public int Id { get; set; }
			public string Login { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
			public string Phone { get; set; }
			public bool IsAdmin { get; set; }
			public bool Disabled { get; set; }
		}

		protected class MetabaseUpdateRightRow {
			public int UserId { get; set; }
			public int BaseId { get; set; }
			public bool CanUpdate { get; set; }
			public string Login { get; set; }
			public string BaseName { get; set; }
		}

		protected class BaseUsersTableRow {
			public int Id { get; set; }
			public string Login { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
			public bool Admin { get; set; }
			public bool Deactivated { get; set; }
		}

		#endregion
	}
}
