using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Список баз и удаление базы: реальный сервер и метабаза должны меняться согласованно.
	/// </summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_DatabasesTest : LauncherDbTestFixtureBase {

		[Test(Description = "Список берётся из метабазы и ограничен выданными доступами")]
		public async Task GetUserDatabases_FromMetabase_ReturnsOnlyGrantedBases() {
			await CreateApplicationDatabase("base_visible", "Видимая");
			await CreateApplicationDatabase("base_hidden", "Скрытая");

			int userId = (await ReadMetabaseUser(RootLogin)).Id;
			int visibleId = await SeedMetabaseBase("base_visible", "Видимая");
			await SeedMetabaseBase("base_hidden", "Скрытая"); // эта в метабазе есть, но доступа на неё нет
			await GrantMetabaseAccess(userId, visibleId);

			var provider = LoginAs();
			var databases = provider.GetUserDatabases();

			Assert.That(databases.Select(d => d.BaseName), Is.EquivalentTo(new[] { "base_visible" }));
			Assert.That(databases[0].Title, Is.EqualTo("Видимая"));
			Assert.That(databases[0].BaseId, Is.EqualTo(visibleId), "идентификатор базы приходит из метабазы");
		}

		[Test(Description = "Пользователь без строк в base_access видит пустой список, а не все базы")]
		public async Task GetUserDatabases_NoAccessRows_ReturnsEmptyWithoutFallback() {
			await CreateApplicationDatabase("base_one");
			await SeedMetabaseBase("base_one");

			var provider = LoginAs();
			var databases = provider.GetUserDatabases();

			// Пустой результат метабазы - это ответ, а не ошибка: отката на прямой сервер не будет.
			Assert.That(databases, Is.Empty,
				"без выданного доступа база не показывается, даже если физически есть на сервере");
		}

		[Test(Description = "Без метабазы список собирается прямо с сервера по base_parameters")]
		public async Task GetUserDatabases_WithoutMetabase_FallsBackToServer() {
			await DropMetabase();
			try {
				await CreateApplicationDatabase("base_direct", "Прямая", version: "2.5");

				var provider = LoginAs();
				var databases = provider.GetUserDatabases();

				var found = databases.FirstOrDefault(d => d.BaseName == "base_direct");
				Assert.That(found, Is.Not.Null, "при отсутствии метабазы работаем напрямую с сервером");
				Assert.That(found?.Title, Is.EqualTo("Прямая"));
				Assert.That(found?.Version, Is.EqualTo("2.5"));
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Прямой список отсеивает базы чужого продукта и базы без параметров")]
		public async Task GetUserDatabases_Direct_SkipsForeignDatabases() {
			await DropMetabase();
			try {
				await CreateApplicationDatabase("base_ours");
				await CreateApplicationDatabase("base_other_product", product: OtherProductCode); // чужой продукт
				await CreateForeignDatabase("base_unrelated"); // вообще не наша база
				await CreateApplicationDatabase("base_no_params", withParameters: false); // опознать нечем

				var provider = LoginAs();
				var names = provider.GetUserDatabases().Select(d => d.BaseName).ToList();

				Assert.That(names, Does.Contain("base_ours"));
				Assert.That(names, Does.Not.Contain("base_other_product"), "чужой продукт не наш");
				Assert.That(names, Does.Not.Contain("base_unrelated"), "без base_parameters база не наша");
				Assert.That(names, Does.Not.Contain("base_no_params"));
				Assert.That(names, Does.Not.Contain(LauncherDbName), "метабаза не прикладная база");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Удаление базы сносит её с сервера и убирает запись из метабазы")]
		public async Task DropDatabase_RemovesFromServerAndMetabase() {
			await CreateApplicationDatabase("base_to_drop");
			int userId = (await ReadMetabaseUser(RootLogin)).Id;
			int baseId = await SeedMetabaseBase("base_to_drop");
			await GrantMetabaseAccess(userId, baseId);

			var provider = LoginAs();
			bool dropped = provider.DropDatabase(new DbInfo { BaseName = "base_to_drop", BaseId = baseId });

			// снимаем состояние всех трёх мест, которых касается удаление
			bool stillOnServer = await DatabaseExists("base_to_drop");
			var metabaseRow = await ReadMetabaseBase("base_to_drop");
			var accessRows = await ReadMetabaseAccess();

			Assert.That(dropped, Is.True);
			Assert.That(stillOnServer, Is.False, "база должна исчезнуть с сервера");
			Assert.That(metabaseRow, Is.Null, "запись в метабазе должна уйти");
			Assert.That(accessRows, Is.Empty, "доступы удаляются вместе с базой");
		}

		[Test(Description = "Удаление базы, известной только по имени, тоже вычищает метабазу")]
		public async Task DropDatabase_WithoutBaseId_ResolvesRecordByName() {
			await CreateApplicationDatabase("base_by_name");
			await SeedMetabaseBase("base_by_name");

			var provider = LoginAs();
			// так база приходит из ветки пересоздания: известно только имя
			provider.DropDatabase(new DbInfo { BaseName = "base_by_name" });

			Assert.That(await ReadMetabaseBase("base_by_name"), Is.Null,
				"запись должна находиться по имени, когда идентификатор неизвестен");
		}

		[Test(Description = "Удаление базы, которой нет в метабазе, не роняет операцию")]
		public async Task DropDatabase_MissingMetabaseRecord_StillDropsDatabase() {
			await CreateApplicationDatabase("base_unknown_to_metabase");

			var provider = LoginAs();
			bool dropped = provider.DropDatabase(new DbInfo { BaseName = "base_unknown_to_metabase" });

			bool stillOnServer = await DatabaseExists("base_unknown_to_metabase");

			Assert.That(dropped, Is.True, "расхождение с метабазой не повод отказать в удалении");
			Assert.That(stillOnServer, Is.False);
		}

		[Test(Description = "Без метабазы удаление базы всё равно проходит")]
		public async Task DropDatabase_WithoutMetabase_Succeeds() {
			await DropMetabase();
			try {
				await CreateApplicationDatabase("base_drop_no_meta");

				var provider = LoginAs();
				bool dropped = provider.DropDatabase(new DbInfo { BaseName = "base_drop_no_meta" });
				bool stillOnServer = await DatabaseExists("base_drop_no_meta");

				Assert.That(dropped, Is.True);
				Assert.That(stillOnServer, Is.False);
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Удаление базы вычищает выданные на неё гранты пользователей")]
		public async Task DropDatabase_CleansUpGrantsOfOtherUsers() {
			await CreateApplicationDatabase("base_with_grants");
			await CreateServerLogin("granted", "granted-pass");
			await GrantOnDatabase("granted", "base_with_grants", "SELECT"); // чужой грант на удаляемую базу

			var provider = LoginAs();
			provider.DropDatabase(new DbInfo { BaseName = "base_with_grants" });

			var grants = await ReadServerGrants("granted");
			Assert.That(GrantsMentionDatabase(grants, "base_with_grants"), Is.False,
				"права на удалённую базу не должны оставаться висеть в mysql.db");
		}

		[Test(Description = "Подключение к базе подставляет её в строку соединения")]
		public async Task LoginToDatabase_ReturnsConnectionStringWithDatabase() {
			await CreateApplicationDatabase("base_to_connect", "Рабочая");

			var provider = LoginAs();
			var response = provider.LoginToDatabase(new DbInfo { BaseName = "base_to_connect", Title = "Рабочая" });

			Assert.That(response.Success, Is.True, response.ErrorMessage);
			Assert.That(response.ConnectionString, Does.Contain("base_to_connect"));
			Assert.That(response.Login, Is.EqualTo(RootLogin));
			Assert.That(response.Parameters["BaseTitle"], Is.EqualTo("Рабочая"));
		}
	}
}
