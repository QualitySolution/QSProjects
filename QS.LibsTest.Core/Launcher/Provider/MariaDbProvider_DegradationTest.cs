using Dapper;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_DegradationTest : LauncherDbTestFixtureBase
	{
		[Test(Description = "Записи о себе в метабазе нет - каталог всё равно читается из неё")]
		public async Task GetUserDatabases_LoginMissingInMetabase_StillUsesCatalog() {
			await CreateApplicationDatabase("base_fallback", "Запасная");
			int baseId = await SeedMetabase("base_fallback", "Из каталога");
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("DELETE FROM `server_users` WHERE login = @login;", new { login = RootLogin });
			}

			var provider = LoginAs();
			var database = provider.GetUserDatabases().FirstOrDefault(d => d.BaseName == "base_fallback");

			Assert.That(database, Is.Not.Null);
			Assert.That(database?.Title, Is.EqualTo("Из каталога"),
				"заголовок пришёл из метабазы - доступность метабазы не зависит от наличия в ней своей записи");
			Assert.That(database?.BaseId, Is.EqualTo(baseId));
		}

		[Test(Description = "Таблицы метабазы нет - откат на прямой сервер, без падения")]
		public async Task GetUserDatabases_MetabaseTableMissing_FallsBackToServer() {
			await CreateApplicationDatabase("base_broken_meta", "Сломанная метабаза");
			using(var connection = CreateConnection(LauncherDbName))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync("DROP TABLE `bases`;"); //метабаза есть, но читать из неё нечего
			}
			try {
				var provider = LoginAs();

				var databases = provider.GetUserDatabases();

				Assert.That(databases.Select(d => d.BaseName), Does.Contain("base_broken_meta"),
					"ошибка чтения метабазы должна уводить в прямой режим");
			}
			finally {
				await DropMetabase();
				await DeployMetabase();
			}
		}

		[Test(Description = "База числится в метабазе, но физически её нет - в списке её не будет")]
		public async Task GetUserDatabases_BaseInMetabaseButNotOnServer_NotListed() {
			await SeedMetabase("base_ghost", "Призрак");

			var provider = LoginAs();

			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Not.Contain("base_ghost"),
				"видимость проверяет сам сервер - несуществующей базы в SHOW DATABASES нет");
		}

		[Test(Description = "Схема users старее нашей - синхронизация строки не проходит, операция не падает")]
		public async Task SetUserBaseAccess_UsersTableWithoutDeactivated_DoesNotThrow() {
			await CreateApplicationDatabase("base_no_deactivated", withDeactivatedColumn: false); // писать в users будет нечем
			int baseId = await SeedMetabase("base_no_deactivated");

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "flagless", Name = "Без флага" }, "flagless-pass");

			var access = new DbUserBaseAccess {
				BaseName = "base_no_deactivated", BaseId = baseId, HasAccess = true, Name = "Без флага"
			};
			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("flagless", access));

			access.HasAccess = false;
			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("flagless", access),
				"схема разошлась - строку в users не пишем вовсе (Warn в логе), но доступ на сервере отзываем");

			var grants = await ReadServerGrants("flagless");
			Assert.That(GrantsMentionDatabase(grants, "`base_no_deactivated`"), Is.False,
				"на сервере права всё равно должны быть отозваны");
		}

		[Test(Description = "Учётка есть на сервере, но не заведена в метабазе - доступы всё равно читаются")]
		public async Task GetUserBaseAccess_UserOnServerOnly_ReadFromGrants() {
			await CreateApplicationDatabase("base_grants_only");
			await SeedMetabase("base_grants_only");
			await CreateServerLogin("outsider", "outsider-pass");
			await GrantOnDatabase("outsider", "base_grants_only", "SELECT");

			var provider = LoginAs();
			var row = provider.GetUserBaseAccess("outsider").FirstOrDefault(r => r.BaseName == "base_grants_only");

			Assert.That(row?.HasAccess, Is.True,
				"пользователя нет в метабазе - доступы приходят из прямого чтения грантов");
		}

		[Test(Description = "Метабаза пропала посреди сессии - следующая операция уходит в прямой режим")]
		public async Task Operations_MetabaseDisappearsMidSession_ContinueDirectly() {
			await CreateApplicationDatabase("base_mid_session");
			await SeedMetabase("base_mid_session");

			var provider = LoginAs();
			Assert.That(provider.GetUserDatabases(), Is.Not.Empty, "предусловие: метабаза работает");

			await DropMetabase();
			try {
				Assert.DoesNotThrow(() => provider.GetUserDatabases(),
					"исчезновение метабазы посреди работы не должно ронять операцию");
				Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Contain("base_mid_session"),
					"список должен собраться напрямую");
			}
			finally {
				await DeployMetabase();
			}
		}
	}
}
