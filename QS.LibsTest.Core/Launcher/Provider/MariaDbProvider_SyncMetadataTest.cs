using Dapper;
using NUnit.Framework;
using QS.DbManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_SyncMetadataTest : LauncherDbTestFixtureBase {

		[Test(Description = "Новая база на сервере попадает в метабазу со всеми параметрами")]
		public async Task RefreshMetadata_NewDatabase_AppearsInMetabase() {
			// база есть на сервере, в метабазе о ней ещё не знают
			await CreateApplicationDatabase("base_fresh", "Свежая", version: "3.1");

			var provider = LoginAs();
			provider.RefreshMetadata();

			var row = await ReadMetabaseBase("base_fresh");
			Assert.That(row, Is.Not.Null, "база должна появиться в метабазе");
			Assert.That(row?.Title, Is.EqualTo("Свежая"));
			Assert.That(row?.Version, Is.EqualTo("3.1"));
			Assert.That(row?.ProductId, Is.EqualTo(TestProductCode));
			Assert.That(row?.Disabled, Is.False);
		}

		[Test(Description = "Базы чужого продукта в нашу метабазу не попадают")]
		public async Task RefreshMetadata_ForeignProduct_NotSynced() {
			await CreateApplicationDatabase("base_ours");
			await CreateApplicationDatabase("base_alien", product: OtherProductCode);
			await CreateForeignDatabase("base_no_params");

			var provider = LoginAs();
			provider.RefreshMetadata();

			var names = (await ReadMetabaseBases()).Select(b => b.BaseName).ToList();
			Assert.That(names, Does.Contain("base_ours"));
			Assert.That(names, Does.Not.Contain("base_alien"));
			Assert.That(names, Does.Not.Contain("base_no_params"));
		}

		[Test(Description = "Пропавшая с сервера база деактивируется, но остаётся в метабазе")]
		public async Task RefreshMetadata_MissingDatabase_SoftDeleted() {
			await CreateApplicationDatabase("base_present");
			await SeedMetabase("base_vanished", "Исчезнувшая"); // только в метабазе, на сервере её нет

			var provider = LoginAs();
			provider.RefreshMetadata();

			var vanished = await ReadMetabaseBase("base_vanished");
			Assert.That(vanished, Is.Not.Null, "синхронизация не удаляет данные, только помечает");
			Assert.That(vanished?.Disabled, Is.True, "пропавшая база должна стать disabled");
			Assert.That(vanished?.Title, Is.EqualTo("Исчезнувшая"), "остальные поля не затираются");
		}

		[Test(Description = "Вернувшаяся база снова становится активной")]
		public async Task RefreshMetadata_ReturnedDatabase_ReactivatedAgain() {
			await SeedMetabase("base_returning", disabled: true); // помечена пропавшей
			await CreateApplicationDatabase("base_returning", "Вернулась"); // и снова появилась на сервере

			var provider = LoginAs();
			provider.RefreshMetadata();

			var row = await ReadMetabaseBase("base_returning");
			Assert.That(row?.Disabled, Is.False, "база снова на сервере - флаг должен сняться");
			Assert.That(row?.Title, Is.EqualTo("Вернулась"), "заголовок должен обновиться из base_parameters");
		}

		[Test(Description = "Повторная синхронизация обновляет запись, а не плодит дубли")]
		public async Task RefreshMetadata_RunTwice_UpdatesSameRow() {
			await CreateApplicationDatabase("base_repeat", "Первый заголовок", version: "1.0");

			var provider = LoginAs();
			provider.RefreshMetadata();
			int idAfterFirst = (await ReadMetabaseBase("base_repeat")).Id;

			await CreateApplicationDatabase("base_repeat", "Второй заголовок", version: "2.0"); // та же база, другие параметры
			provider.RefreshMetadata();

			var rows = (await ReadMetabaseBases()).Where(b => b.BaseName == "base_repeat").ToList();
			Assert.That(rows, Has.Count.EqualTo(1), "upsert обязан обновлять, а не вставлять второй раз");
			Assert.That(rows[0].Id, Is.EqualTo(idAfterFirst), "идентификатор записи сохраняется");
			Assert.That(rows[0].Title, Is.EqualTo("Второй заголовок"));
			Assert.That(rows[0].Version, Is.EqualTo("2.0"));
		}

		[Test(Description = "метабаза развёрнута, но пуста - синхронизация обязана пройти")]
		public async Task RefreshMetadata_EmptyMetabase_PopulatesUsers() {
			await CreateServerLogin("firstcomer", "first-pass");
			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				// состояние сразу после развёртывания скрипта: ни одного пользователя
				await connection.ExecuteAsync("DELETE FROM `server_users`;");
			}

			var provider = LoginAs();
			var response = provider.RefreshMetadata();

			Assert.That(response.Success, Is.True,
				"до 24.08.2026 здесь был замкнутый круг: нет записи о себе - нет метабазы - нечем синхронизировать");
			Assert.That((await ReadMetabaseUsers()).Select(u => u.Login), Does.Contain("firstcomer"),
				"пустая метабаза должна наполняться этой же кнопкой, а не руками через SQL");
		}

		[Test(Description = "Права администратора у новой учётки берутся с сервера")]
		public async Task RefreshMetadata_NewAdminOnServer_KeepsAdminFlag() {
			await CreateServerLogin("newchief", "chief-pass", isAdmin: true);

			var provider = LoginAs();
			provider.RefreshMetadata();

			Assert.That((await ReadMetabaseUser("newchief"))?.IsAdmin, Is.True,
				"иначе настоящий администратор сервера получил бы запись с is_admin = 0 "
				+ "и потерял управление пользователями");
		}

		[Test(Description = "Служебную учётку сервера синхронизация не гасит - её просто не показывают")]
		public async Task RefreshMetadata_SystemAccount_NotDisabled() {
			var provider = LoginAs(); // вход под root, его запись в метабазе завела фикстура

			provider.RefreshMetadata();

			Assert.That((await ReadMetabaseUser(RootLogin))?.Disabled, Is.False,
				"root скрыт из списка пользователей, но на сервере он есть - гасить его запись нельзя");
		}

		[Test(Description = "Без метабазы синхронизация тихо ничего не делает")]
		public async Task RefreshMetadata_WithoutMetabase_DoesNothingQuietly() {
			await DropMetabase();
			try {
				await CreateApplicationDatabase("base_orphan");
				var provider = LoginAs();

				Assert.DoesNotThrow(() => provider.RefreshMetadata(),
					"метабаза необязательна - её отсутствие не ошибка");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Пользователю без прав на запись метабаза отказывает явно")]
		public async Task RefreshMetadata_UserWithoutWriteRights_Throws() {
			await CreateServerLogin("reader", "reader-pass");
			await SeedMetabaseUser("reader", isAdmin: true);
			await GrantOnDatabase("reader", LauncherDbName, "SELECT"); // метабазу читает, но не пишет
			await CreateApplicationDatabase("base_any");

			var provider = LoginAs("reader", "reader-pass");

			Assert.That(provider.CanRefreshMetadata, Is.False,
				"кнопка синхронизации не должна быть доступна пользователю без права создавать базы");
			Assert.Throws<UnauthorizedAccessException>(() => provider.RefreshMetadata(),
				"если операцию всё-таки позвали - отказ должен быть явным");
		}

		[Test(Description = "Пользователю, который видит не весь сервер, синхронизация запрещена")]
		public async Task RefreshMetadata_UserSeeingPartOfServer_Throws() {
			await CreateApplicationDatabase("base_his_own");
			await CreateApplicationDatabase("base_someone_elses");
			await SeedMetabase("base_his_own");
			await SeedMetabase("base_someone_elses");

			// права есть, но только на свою базу: CREATE у него есть, а сервер он видит не весь
			await CreateServerLogin("owner", "owner-pass");
			await GrantOnDatabase("owner", LauncherDbName, "SELECT, INSERT, UPDATE, DELETE");
			await GrantOnDatabase("owner", "base_his_own", "ALL PRIVILEGES");
			await SeedMetabaseUser("owner", isAdmin: true);

			var provider = LoginAs("owner", "owner-pass");

			Assert.That(provider.CanRefreshMetadata, Is.False,
				"SHOW DATABASES покажет ему не все базы - синхронизировать каталог он не вправе");
			Assert.Throws<UnauthorizedAccessException>(() => provider.RefreshMetadata(),
				"если операцию всё-таки позвали - отказ должен быть явным");

			//невидимые базы неотличимы от удалённых, и синхронизация погасила бы их
			Assert.That((await ReadMetabaseBase("base_someone_elses"))?.Disabled, Is.False,
				"чужая база не должна быть помечена пропавшей");
		}
	}
}
