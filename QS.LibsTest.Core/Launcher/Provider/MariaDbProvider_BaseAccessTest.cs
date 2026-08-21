using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Доступ пользователя к базе живёт сразу в трёх местах: гранты сервера, base_access метабазы
	/// и строка в таблице users самой базы. Проверяем, что они не расходятся.
	/// </summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_BaseAccessTest : LauncherDbTestFixtureBase {
		private const string BaseName = "base_access_test";

		private MariaDBProvider provider;
		private int baseId;

		[SetUp]
		public async Task SetUpScenario() {
			await CreateApplicationDatabase(BaseName, "Тестовая база");
			baseId = await SeedMetabaseBase(BaseName, "Тестовая база");

			int rootId = (await ReadMetabaseUser(RootLogin)).Id;
			await GrantMetabaseAccess(rootId, baseId, admin: true);

			provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "worker", Name = "Работник" }, "worker-pass");
		}

		private DbUserBaseAccess Access(bool hasAccess = true, bool isAdmin = false, bool readOnly = false) =>
			new DbUserBaseAccess {
				BaseName = BaseName,
				BaseId = baseId,
				HasAccess = hasAccess,
				IsAdmin = isAdmin,
				ReadOnly = readOnly,
				Name = "Работник",
				Email = "worker@example.com"
			};

		[Test(Description = "Выдача доступа отражается в грантах, метабазе и таблице users базы")]
		public async Task SetUserBaseAccess_Granted_UpdatesAllThreePlaces() {
			provider.SetUserBaseAccess("worker", Access());

			// те самые три места
			var grants = await ReadServerGrants("worker");
			var accessRows = await ReadMetabaseAccess();
			var baseUser = await ReadBaseUser(BaseName, "worker");

			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.True,
				"на сервере должен появиться грант на базу");
			Assert.That(accessRows.Any(r => r.Login == "worker" && r.BaseName == BaseName), Is.True,
				"в метабазе должна появиться строка base_access");
			Assert.That(baseUser, Is.Not.Null, "в самой базе должен появиться пользователь");
			Assert.That(baseUser?.Deactivated, Is.False);
			Assert.That(baseUser?.Name, Is.EqualTo("Работник"), "профиль пишется в users базы");
			Assert.That(baseUser?.Email, Is.EqualTo("worker@example.com"));
		}

		[Test(Description = "Доступ только на чтение выдаёт лишь читающие привилегии")]
		public async Task SetUserBaseAccess_ReadOnly_GrantsSelectOnly() {
			provider.SetUserBaseAccess("worker", Access(readOnly: true));

			var grants = await ReadServerGrants("worker");
			string baseGrant = FindGrantOnDatabase(grants, BaseName);

			Assert.That(baseGrant, Is.Not.Null);
			Assert.That(baseGrant, Does.Contain("SELECT"));
			Assert.That(baseGrant, Does.Not.Contain("INSERT"), "на чтение - значит без записи");
			Assert.That(baseGrant, Does.Not.Contain("DELETE"));
		}

		[Test(Description = "Администратор базы получает на неё все права")]
		public async Task SetUserBaseAccess_BaseAdmin_GrantsAllPrivilegesOnBase() {
			provider.SetUserBaseAccess("worker", Access(isAdmin: true));

			var grants = await ReadServerGrants("worker");
			string baseGrant = FindGrantOnDatabase(grants, BaseName);
			var accessRow = (await ReadMetabaseAccess()).FirstOrDefault(r => r.Login == "worker");

			Assert.That(baseGrant, Does.Contain("ALL PRIVILEGES"));
			Assert.That(accessRow?.Admin, Is.True, "флаг администратора базы должен уйти в метабазу");
		}

		[Test(Description = "Снятие доступа убирает гранты, строку метабазы и деактивирует пользователя в базе")]
		public async Task SetUserBaseAccess_Revoked_CleansUpEverywhere() {
			provider.SetUserBaseAccess("worker", Access());
			provider.SetUserBaseAccess("worker", Access(hasAccess: false)); // и сразу отбираем

			var grants = await ReadServerGrants("worker");
			var accessRows = await ReadMetabaseAccess();
			var baseUser = await ReadBaseUser(BaseName, "worker");

			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.False,
				"грант на базу должен быть отозван");
			Assert.That(accessRows.Any(r => r.Login == "worker" && r.BaseName == BaseName), Is.False,
				"строка base_access должна исчезнуть");
			Assert.That(baseUser, Is.Not.Null, "саму строку пользователя в базе не удаляем");
			Assert.That(baseUser?.Deactivated, Is.True, "доступ снимается флагом deactivated");
		}

		[Test(Description = "Повторная выдача доступа не плодит гранты и строки")]
		public async Task SetUserBaseAccess_AppliedTwice_IsIdempotent() {
			provider.SetUserBaseAccess("worker", Access());
			provider.SetUserBaseAccess("worker", Access()); // тот же доступ второй раз

			var accessRows = (await ReadMetabaseAccess()).Where(r => r.Login == "worker").ToList();
			var baseUsers = (await ReadBaseUsers(BaseName)).Where(u => u.Login == "worker").ToList();

			Assert.That(accessRows, Has.Count.EqualTo(1), "в base_access должна быть одна строка");
			Assert.That(baseUsers, Has.Count.EqualTo(1), "в users базы - тоже одна");
		}

		[Test(Description = "Список доступов из метабазы показывает все базы продукта с флагами")]
		public async Task GetUserBaseAccess_FromMetabase_ListsAllProductBasesWithFlags() {
			await CreateApplicationDatabase("base_second", "Вторая");
			await SeedMetabaseBase("base_second", "Вторая"); // вторая база продукта, доступа на неё не будет

			provider.SetUserBaseAccess("worker", Access());

			var rows = provider.GetUserBaseAccess("worker");

			Assert.That(rows.Select(r => r.BaseName), Is.EquivalentTo(new[] { BaseName, "base_second" }),
				"показываем все базы продукта, а не только доступные");
			Assert.That(rows.First(r => r.BaseName == BaseName).HasAccess, Is.True);
			Assert.That(rows.First(r => r.BaseName == "base_second").HasAccess, Is.False);
		}

		[Test(Description = "Без метабазы доступы вычисляются из реальных грантов сервера")]
		public async Task GetUserBaseAccess_WithoutMetabase_ComputedFromGrants() {
			await GrantOnDatabase("worker", BaseName, "SELECT, LOCK TABLES, SHOW VIEW");
			await DropMetabase();
			try {
				var direct = LoginAs();
				var rows = direct.GetUserBaseAccess("worker");

				var row = rows.FirstOrDefault(r => r.BaseName == BaseName);
				Assert.That(row, Is.Not.Null);
				Assert.That(row?.HasAccess, Is.True);
				Assert.That(row?.ReadOnly, Is.True, "только читающие привилегии - значит доступ на чтение");
				Assert.That(row?.IsAdmin, Is.False);
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Без метабазы база без грантов показывается без доступа")]
		public async Task GetUserBaseAccess_WithoutMetabase_BaseWithoutGrantsHasNoAccess() {
			await CreateApplicationDatabase("base_no_grants", "Без грантов"); // грантов на неё не выдаём
			await GrantOnDatabase("worker", BaseName, "SELECT, INSERT"); // а на соседнюю - выдаём
			await DropMetabase();
			try {
				var direct = LoginAs();
				var rows = direct.GetUserBaseAccess("worker");

				// у любой учётки есть GRANT USAGE ON *.*, и он не должен читаться как доступ
				Assert.That(rows.First(r => r.BaseName == "base_no_grants").HasAccess, Is.False,
					"USAGE - это отсутствие привилегий, а не доступ ко всем базам");
				Assert.That(rows.First(r => r.BaseName == BaseName).HasAccess, Is.True,
					"предусловие: там, где гранты есть, доступ виден");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "У глобального администратора доступ ко всем базам и правится он не здесь")]
		public async Task GetUserBaseAccess_GlobalAdmin_ReportsFullAccessNotEditable() {
			await CreateServerLogin("superuser", "super-pass", isAdmin: true); // права на весь сервер
			await DropMetabase();
			try {
				var direct = LoginAs();
				var rows = direct.GetUserBaseAccess("superuser");

				var row = rows.FirstOrDefault(r => r.BaseName == BaseName);
				Assert.That(row?.HasAccess, Is.True);
				Assert.That(row?.IsAdmin, Is.True);
				Assert.That(row?.CanEdit, Is.False,
					"права выданы на весь сервер - побазово их редактировать нельзя");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Менять доступ глобальному администратору запрещено явной ошибкой")]
		public async Task SetUserBaseAccess_GlobalAdmin_Throws() {
			await CreateServerLogin("superuser", "super-pass", isAdmin: true);

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess("superuser", Access()));

			Assert.That(exception.Message, Does.Contain("глобальные права"));
		}

		[Test(Description = "Доступ несуществующему пользователю - явная ошибка, а не молчание")]
		public void SetUserBaseAccess_UnknownLogin_Throws() {
			Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess("who_is_this", Access()));
		}

		[Test(Description = "Пустое имя базы отвергается до обращения к серверу")]
		public void SetUserBaseAccess_EmptyBaseName_Throws() {
			Assert.Throws<ArgumentException>(() => provider.SetUserBaseAccess("worker",
				new DbUserBaseAccess { BaseName = string.Empty, HasAccess = true }));
		}

		[Test(Description = "База без таблицы users - гранты выдаются, синхронизация профиля молча пропускается")]
		public async Task SetUserBaseAccess_BaseWithoutUsersTable_StillGrantsOnServer() {
			await CreateApplicationDatabase("base_no_users", withUsersTable: false);
			int noUsersId = await SeedMetabaseBase("base_no_users");

			Assert.DoesNotThrow(() => provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseName = "base_no_users", BaseId = noUsersId, HasAccess = true, Name = "Работник"
			}), "отсутствие таблицы users в базе не должно ломать выдачу доступа");

			var grants = await ReadServerGrants("worker");
			Assert.That(GrantsMentionDatabase(grants, "base_no_users"), Is.True);
		}

		[Test(Description = "Профиль без имени подставляет логин - колонка name в базах NOT NULL")]
		public async Task SetUserBaseAccess_WithoutName_FallsBackToLogin() {
			provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseName = BaseName, BaseId = baseId, HasAccess = true, Name = null
			});

			var baseUser = await ReadBaseUser(BaseName, "worker");
			Assert.That(baseUser?.Name, Is.EqualTo("worker"),
				"пустое имя должно замещаться логином, иначе вставка упадёт на NOT NULL");
		}

		[Test(Description = "Расхождение: доступ есть в метабазе, но грантов на сервере нет")]
		public async Task GetUserBaseAccess_MetabaseAndServerDisagree_MetabaseWins() {
			int workerId = (await ReadMetabaseUser("worker")).Id;
			// доступ проставлен только в метабазе, минуя сервер - расхождение собрано вручную
			await GrantMetabaseAccess(workerId, baseId);

			var rows = provider.GetUserBaseAccess("worker");
			var row = rows.FirstOrDefault(r => r.BaseName == BaseName);

			var grants = await ReadServerGrants("worker");
			Assert.That(row?.HasAccess, Is.True,
				"когда метабаза доступна, показания снимаются с неё - это её роль как источника правды");
			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.False,
				"предусловие теста: на сервере гранта действительно нет");
		}
	}
}
