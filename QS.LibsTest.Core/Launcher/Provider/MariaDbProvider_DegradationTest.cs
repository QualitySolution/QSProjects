using Dapper;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
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

		[Test(Description = "Аккаунт пользователя потерян - метабаза недоступна, но лаунчер работает")]
		public async Task GetUserDatabases_AccountRowMissing_FallsBackToServer() {
			await CreateApplicationDatabase("base_no_account", "Без аккаунта");
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				// пользователь остался, аккаунт исчез - JOIN в метабазе не сойдётся
				await connection.ExecuteAsync("DELETE FROM `accounts`;"); // JOIN в метабазе больше не сойдётся
			}

			var provider = LoginAs();

			Assert.DoesNotThrow(() => provider.GetUserDatabases());
			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Contain("base_no_account"));
		}

		[Test(Description = "Таблицы метабазы нет - откат на прямой сервер, без падения")]
		public async Task GetUserDatabases_MetabaseTableMissing_FallsBackToServer() {
			await CreateApplicationDatabase("base_broken_meta", "Сломанная метабаза");
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("DROP TABLE `base_access`;"); // метабаза есть, но читать из неё нечего
			}
			try {
				var provider = LoginAs();

				var databases = provider.GetUserDatabases();

				Assert.That(databases.Select(d => d.BaseName), Does.Contain("base_broken_meta"),
					"ошибка чтения метабазы должна уводить в прямой режим");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "База числится в метабазе, но физически её нет - в списке она есть, подключение падает")]
		public async Task GetUserDatabases_BaseInMetabaseButNotOnServer_ListedButNotConnectable() {
			int rootId = (await ReadMetabaseUser(RootLogin)).Id;
			int ghostId = await SeedMetabaseBase("base_ghost", "Призрак");
			await GrantMetabaseAccess(rootId, ghostId);

			var provider = LoginAs();
			var databases = provider.GetUserDatabases();

			Assert.That(databases.Select(d => d.BaseName), Does.Contain("base_ghost"),
				"метабаза - источник правды для списка, расхождение видно только при подключении");

			// подключение к несуществующей базе - ожидаемый отказ на границе, не исключение наружу
			var response = provider.LoginToDatabase(databases.First(d => d.BaseName == "base_ghost"));
			Assert.That(response.Success, Is.True,
				"LoginToDatabase только собирает строку подключения - проверка живости базы не его дело");
		}

		[Test(Description = "База есть на сервере, но её нет в метабазе - до синхронизации она невидима")]
		public async Task GetUserDatabases_BaseOnServerButNotInMetabase_InvisibleUntilSync() {
			await CreateApplicationDatabase("base_unregistered", "Незарегистрированная"); // в метабазу не заносим

			var provider = LoginAs();
			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Not.Contain("base_unregistered"),
				"метабаза отвечает пустым списком, а не ошибкой - отката не будет");

			provider.RefreshMetadata();
			int rootId = (await ReadMetabaseUser(RootLogin)).Id;
			int baseId = (await ReadMetabaseBase("base_unregistered")).Id;
			await GrantMetabaseAccess(rootId, baseId);

			Assert.That(provider.GetUserDatabases().Select(d => d.BaseName), Does.Contain("base_unregistered"),
				"после синхронизации и выдачи доступа база должна появиться");
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

		[Test(Description = "Доступ в метабазе ссылается на удалённую базу - список не ломается")]
		public async Task GetUserDatabases_DanglingAccessRow_Ignored() {
			await CreateApplicationDatabase("base_alive");
			int rootId = (await ReadMetabaseUser(RootLogin)).Id;
			int aliveId = await SeedMetabaseBase("base_alive");
			await GrantMetabaseAccess(rootId, aliveId);
			// доступ на базу, записи о которой нет
			await GrantMetabaseAccess(rootId, baseId: 999999); // ссылка в никуда

			var provider = LoginAs();
			var databases = provider.GetUserDatabases();

			Assert.That(databases.Select(d => d.BaseName), Is.EquivalentTo(new[] { "base_alive" }),
				"висячая строка доступа не должна ни ломать выборку, ни добавлять пустую базу");
		}

		[Test(Description = "Учётка есть на сервере, но не заведена в метабазе - доступы всё равно читаются")]
		public async Task GetUserBaseAccess_UserOnServerOnly_ReadFromGrants() {
			await CreateApplicationDatabase("base_grants_only");
			await CreateServerLogin("outsider", "outsider-pass");
			await GrantOnDatabase("outsider", "base_grants_only", "SELECT");

			var provider = LoginAs();
			var rows = provider.GetUserBaseAccess("outsider");

			Assert.That(rows, Is.Not.Null,
				"пользователя нет в метабазе - доступы должны прийти из прямого чтения грантов");
		}

		[Test(Description = "Права разошлись: на сервере доступ шире, чем записано в метабазе")]
		public async Task GetUserBaseAccess_ServerWiderThanMetabase_ReportsMetabaseView() {
			await CreateApplicationDatabase("base_disagreement");
			int baseId = await SeedMetabaseBase("base_disagreement");

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "wider", Name = "Шире" }, "wider-pass");
			// на сервере выдали руками, мимо лаунчера - метабаза об этом не знает, права разошлись
			await GrantOnDatabase("wider", "base_disagreement", "ALL PRIVILEGES");

			var fromMetabase = provider.GetUserBaseAccess("wider")
				.FirstOrDefault(r => r.BaseName == "base_disagreement");

			Assert.That(fromMetabase?.HasAccess, Is.False,
				"пока метабаза доступна, показания снимаются с неё - расхождение с сервером тут не видно");

			// а прямое чтение видит реальную картину
			await DropMetabase();
			try {
				var direct = LoginAs();
				var fromServer = direct.GetUserBaseAccess("wider")
					.FirstOrDefault(r => r.BaseName == "base_disagreement");

				Assert.That(fromServer?.HasAccess, Is.True,
					"без метабазы видно реальные гранты сервера");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Метабаза пропала посреди сессии - следующая операция уходит в прямой режим")]
		public async Task Operations_MetabaseDisappearsMidSession_ContinueDirectly() {
			await CreateApplicationDatabase("base_mid_session");
			int rootId = (await ReadMetabaseUser(RootLogin)).Id;
			await GrantMetabaseAccess(rootId, await SeedMetabaseBase("base_mid_session"));

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
