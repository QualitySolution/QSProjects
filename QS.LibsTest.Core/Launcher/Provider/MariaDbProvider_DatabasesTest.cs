using NSubstitute;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Creation;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_DatabasesTest : LauncherDbTestFixtureBase
	{
		[Test(Description = "Список берётся из метабазы со всеми полями записи")]
		public async Task GetUserDatabases_FromMetabase_ReturnsCatalogRow() {
			await CreateApplicationDatabase("base_listed", "Видимая");
			int baseId = await SeedMetabase("base_listed", "Видимая", version: "2.5");

			var provider = LoginAs();
			var found = provider.GetUserDatabases().FirstOrDefault(d => d.BaseName == "base_listed");

			Assert.That(found, Is.Not.Null);
			Assert.That(found?.Title, Is.EqualTo("Видимая"));
			Assert.That(found?.Version, Is.EqualTo("2.5"));
			Assert.That(found?.BaseId, Is.EqualTo(baseId), "идентификатор базы приходит из метабазы");
		}

		[Test(Description = "База, на которую у пользователя нет прав, в списке не показывается")]
		public async Task GetUserDatabases_BaseInvisibleToUser_NotListed() {
			await CreateApplicationDatabase("base_allowed", "Своя");
			await CreateApplicationDatabase("base_forbidden", "Чужая");
			await SeedMetabase("base_allowed", "Своя");
			await SeedMetabase("base_forbidden", "Чужая"); // в каталоге есть, прав на неё не будет

			await CreateServerLogin("limited", "limited-pass");
			await GrantOnDatabase("limited", LauncherDbName, "SELECT"); // метабазу читать надо
			await GrantOnDatabase("limited", "base_allowed", "SELECT");
			await SeedMetabaseUser("limited");

			var names = LoginAs("limited", "limited-pass").GetUserDatabases().Select(d => d.BaseName).ToList();

			Assert.That(names, Does.Contain("base_allowed"));
			Assert.That(names, Does.Not.Contain("base_forbidden"),
				"видимость определяет сервер: SHOW DATABASES не покажет базу без прав");
			Assert.That(names, Does.Not.Contain(LauncherDbName), "метабаза не прикладная база");
		}

		[Test(Description = "Без метабазы список собирается прямо с сервера по base_parameters")]
		public async Task GetUserDatabases_WithoutMetabase_FallsBackToServer() {
			await DropMetabase();
			try {
				await CreateApplicationDatabase("base_direct", "Прямая", version: "2.5");

				var found = LoginAs().GetUserDatabases().FirstOrDefault(d => d.BaseName == "base_direct");

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

				var names = LoginAs().GetUserDatabases().Select(d => d.BaseName).ToList();

				Assert.That(names, Does.Contain("base_ours"));
				Assert.That(names, Does.Not.Contain("base_other_product"), "чужой продукт не наш");
				Assert.That(names, Does.Not.Contain("base_unrelated"), "без base_parameters база не наша");
				Assert.That(names, Does.Not.Contain("base_no_params"));
			}
			finally {
				await DeployMetabase();
			}
		}

		#region Создание базы

		private const string DumpVersion = "3.7";

		[Test(Description = "Созданная база попадает в каталог метабазы вместе с версией из своих параметров")]
		public async Task CreateDatabase_NewBase_RegistersInMetabaseWithVersion() {
			var provider = LoginAs();

			bool created = await CreateBase(provider, "base_created", "Созданная");

			Assert.That(created, Is.True);
			var row = await ReadMetabaseBase("base_created");
			Assert.That(row, Is.Not.Null, "созданная база должна попасть в каталог");
			Assert.That(row?.Title, Is.EqualTo("Созданная"));
			Assert.That(row?.ProductId, Is.EqualTo(TestProductCode));
			Assert.That(row?.Version, Is.EqualTo(DumpVersion),
				"версию база сообщает о себе сама - её кладёт в base_parameters дамп, которым базу наполнили");
		}

		[Test(Description = "Перезапись базы сохраняет запись каталога и выданные на базу доступы")]
		public async Task CreateDatabase_RewriteExisting_KeepsCatalogRowAndAccess() {
			await CreateApplicationDatabase("base_rewritten", "Старая");
			int baseId = await SeedMetabase("base_rewritten", "Старая");

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "colleague", Name = "Коллега" }, "colleague-pass");
			provider.SetUserBaseAccess("colleague", new DbUserBaseAccess {
				BaseName = "base_rewritten", BaseId = baseId, HasAccess = true, Name = "Коллега"
			});

			bool created = await CreateBase(provider, "base_rewritten", "Новая",
				ToDoWithExistingDatabase.Rewrite, "4.0");

			Assert.That(created, Is.True);

			var rows = (await ReadMetabaseBases()).Where(b => b.BaseName == "base_rewritten").ToList();
			var grants = await ReadServerGrants("colleague");
			var rights = await ReadBaseUpdateRights();

			Assert.Multiple(() => {
				Assert.That(rows, Has.Count.EqualTo(1), "запись должна пережить перезапись, а не задвоиться");
				Assert.That(rows.FirstOrDefault()?.Id, Is.EqualTo(baseId),
					"идентификатор сохраняется - на нём висят права");
				Assert.That(rows.FirstOrDefault()?.Title, Is.EqualTo("Новая"));
				Assert.That(rows.FirstOrDefault()?.Version, Is.EqualTo("4.0"),
					"версия должна стать версией нового дампа");

				// ради этого перезапись и отличается от пересоздания
				Assert.That(GrantsMentionDatabase(grants, "base_rewritten"), Is.True,
					"грант коллеги должен пережить перезапись - иначе после обновления базы все теряют доступ");
				Assert.That(rights.Any(r => r.Login == "colleague" && r.BaseName == "base_rewritten"), Is.True,
					"право на обновление базы тоже должно остаться");
			});
		}

		private async Task<bool> CreateBase(MariaDBProvider provider, string dbName, string title,
			ToDoWithExistingDatabase onExisting = ToDoWithExistingDatabase.Nothing, string version = DumpVersion) {
			string scriptPath = await WriteDumpScript(version);
			try {
				return provider.CreateDatabase(CreationRequest(dbName, title, onExisting, scriptPath, version));
			}
			finally {
				File.Delete(scriptPath);
			}
		}

		private static DbCreationRequest CreationRequest(string dbName, string title,
			ToDoWithExistingDatabase onExisting, string scriptPath, string version) {
			var map = new DbResourcesCreationMap();
			map.Register(typeof(EmbeddedCreationResources), typeof(MySqlDbCreateModel));

			var interaction = Substitute.For<IDbCreatorInteraction>();
			interaction.AskDropExistingDatabase(Arg.Any<string>()).Returns(onExisting);

			return new DbCreationRequest {
				DbName = dbName,
				DbTitle = title,
				Interaction = interaction,
				CreationFactory = new DbCreationFactory(map),
				CreationResources = new EmbeddedCreationResources {
					Script = new CreationScript(scriptPath, new Version(version)),
					Interactions = interaction,
					Progress = Substitute.For<IProgressBarDisplayable>()
				}
			};
		}

		/// <summary>
		/// схема плюс параметры, по которым базу потом опознают
		/// </summary>
		private static async Task<string> WriteDumpScript(string version) {
			string sql = await ReadScript("AppBase.sql")
				+ "\nINSERT INTO `base_parameters` (name, str_value) VALUES "
				+ $"('ProductCode', '{TestProductCode}'), ('version', '{version}');";

			string path = Path.Combine(Path.GetTempPath(), $"qs-launcher-dump-{Guid.NewGuid():N}.sql");
			File.WriteAllText(path, sql);
			return path;
		}

		#endregion

		[Test(Description = "Удаление базы сносит её с сервера и убирает запись из метабазы")]
		public async Task DropDatabase_RemovesFromServerAndMetabase() {
			await CreateApplicationDatabase("base_to_drop");
			int userId = (await ReadMetabaseUser(RootLogin)).Id;
			int baseId = await SeedMetabase("base_to_drop");
			await GrantBaseUpdateRight(userId, baseId);

			var provider = LoginAs();
			bool dropped = provider.DropDatabase(new DbInfo { BaseName = "base_to_drop", BaseId = baseId });

			// снимаем состояние всех трёх мест, которых касается удаление
			bool stillOnServer = await DatabaseExists("base_to_drop");
			var metabaseRow = await ReadMetabaseBase("base_to_drop");
			var rights = await ReadBaseUpdateRights();

			Assert.That(dropped, Is.True);
			Assert.That(stillOnServer, Is.False, "база должна исчезнуть с сервера");
			Assert.That(metabaseRow, Is.Null, "запись в метабазе должна уйти");
			Assert.That(rights, Is.Empty, "права на обновление удаляются вместе с базой");
		}

		[Test(Description = "Удаление базы, известной только по имени, тоже вычищает метабазу")]
		public async Task DropDatabase_WithoutBaseId_ResolvesRecordByName(){
			await CreateApplicationDatabase("base_by_name");
			await SeedMetabase("base_by_name");

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
			await GrantOnDatabase("granted", "base_with_grants", "SELECT"); //чужой грант на удаляемую базу

			var provider = LoginAs();
			provider.DropDatabase(new DbInfo { BaseName = "base_with_grants" });

			var grants = await ReadServerGrants("granted");
			Assert.That(GrantsMentionDatabase(grants, "base_with_grants"), Is.False,
				"права на удалённую базу не должны оставаться висеть в mysql.db");
		}
	}
}
