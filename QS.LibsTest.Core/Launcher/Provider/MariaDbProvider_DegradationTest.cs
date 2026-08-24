using Dapper;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Края: чего-то не хватает в метабазе или в самих базах, данные разошлись.
	/// Правило одно - необязательная подсистема не имеет права ронять основную операцию.
	/// </summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_DegradationTest : LauncherDbTestFixtureBase {

		[Test(Description = "Пользователя нет в метабазе - работаем напрямую с сервером")]
		public async Task GetUserDatabases_LoginMissingInMetabase_FallsBackToServer() {
			await CreateApplicationDatabase("base_fallback", "Запасная");
			// вычищаем запись о себе из метабазы: метаданные собрать не получится
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("DELETE FROM `server_users` WHERE login = @login;", new { login = RootLogin });
			}

			var provider = LoginAs();
			var databases = provider.GetUserDatabases();

			Assert.That(databases.Select(d => d.BaseName), Does.Contain("base_fallback"),
				"метабазой воспользоваться нельзя - список должен собраться прямым запросом");
		}

		[Test(Description = "Таблицы метабазы нет - откат на прямой сервер, без падения")]
		public async Task GetUserDatabases_MetabaseTableMissing_FallsBackToServer() {
			await CreateApplicationDatabase("base_broken_meta", "Сломанная метабаза");
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("DROP TABLE `bases`;"); // метабаза есть, но читать из неё нечего
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
			await SeedMetabaseBase("base_ghost", "Призрак");

			var provider = LoginAs();

			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Not.Contain("base_ghost"),
				"видимость проверяет сам сервер - несуществующей базы в SHOW DATABASES нет");
		}

		[Test(Description = "База есть на сервере, но её нет в метабазе - до синхронизации она невидима")]
		public async Task GetUserDatabases_BaseOnServerButNotInMetabase_InvisibleUntilSync() {
			await CreateApplicationDatabase("base_unregistered", "Незарегистрированная"); // в метабазу не заносим

			var provider = LoginAs();
			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Not.Contain("base_unregistered"),
				"метабаза отвечает пустым списком, а не ошибкой - отката не будет");

			provider.RefreshMetadata();

			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Contain("base_unregistered"),
				"после синхронизации база должна появиться");
		}

		[Test(Description = "В users базы нет колонки deactivated - снятие доступа не падает")]
		public async Task SetUserBaseAccess_UsersTableWithoutDeactivated_DoesNotThrow() {
			await CreateApplicationDatabase("base_no_deactivated", withDeactivatedColumn: false); // снимать доступ будет нечем
			int baseId = await SeedMetabaseBase("base_no_deactivated");

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "flagless", Name = "Без флага" }, "flagless-pass");

			var access = new DbUserBaseAccess {
				BaseName = "base_no_deactivated", BaseId = baseId, HasAccess = true, Name = "Без флага"
			};
			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("flagless", access));

			access.HasAccess = false;
			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("flagless", access),
				"колонки нет - синхронизацию профиля пропускаем, но доступ на сервере отзываем");

			var grants = await ReadServerGrants("flagless");
			Assert.That(GrantsMentionDatabase(grants, "`base_no_deactivated`"), Is.False,
				"на сервере права всё равно должны быть отозваны");
		}

		[Test(Description = "База не заведена в метабазе - гранты выдаются, право записать некуда")]
		public async Task SetUserBaseAccess_BaseMissingInMetabase_GrantsStillIssued() {
			await CreateApplicationDatabase("base_unregistered"); // в метабазу не заносим

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "nobase", Name = "Без базы" }, "nobase-pass");

			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("nobase", new DbUserBaseAccess {
				BaseName = "base_unregistered", HasAccess = true, Name = "Без базы"
			}), "отражение в метабазе - best-effort, гранты выдать оно мешать не должно");

			var grants = await ReadServerGrants("nobase");
			Assert.That(GrantsMentionDatabase(grants, "base_unregistered"), Is.True);
			Assert.That(await ReadBaseUpdateRights(), Is.Empty,
				"право на неизвестную базу записать некуда - строка ссылается на bases.id");
		}

		[Test(Description = "База без ProductCode в параметрах в синхронизацию не попадает")]
		public async Task RefreshMetadata_BaseWithoutProductCode_Skipped() {
			await CreateApplicationDatabase("base_no_product", withParameters: false);
			await CreateApplicationDatabase("base_with_product");

			var provider = LoginAs();
			provider.RefreshMetadata();

			var names = (await ReadMetabaseBases()).Select(b => b.BaseName).ToList();
			Assert.That(names, Does.Not.Contain("base_no_product"),
				"без ProductCode базу нельзя отнести к продукту");
			Assert.That(names, Does.Contain("base_with_product"));
		}

		[Test(Description = "Учётка есть на сервере, но не заведена в метабазе - доступы всё равно читаются")]
		public async Task GetUserBaseAccess_UserOnServerOnly_ReadFromGrants() {
			await CreateApplicationDatabase("base_grants_only");
			await SeedMetabaseBase("base_grants_only");
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
			await SeedMetabaseBase("base_mid_session");

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
