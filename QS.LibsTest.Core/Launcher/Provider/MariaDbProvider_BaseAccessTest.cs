using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Доступ пользователя к базе живёт в грантах сервера и в строке таблицы users самой базы.
	/// Право накатывать обновления - это выданный набор привилегий с DDL, метабаза лишь
	/// отражает его у себя. Проверяем, что эти три вещи не расходятся.
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
			var rights = await ReadBaseUpdateRights();
			var baseUser = await ReadBaseUser(BaseName, "worker");

			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.True,
				"на сервере должен появиться грант на базу");
			Assert.That(rights.Any(r => r.Login == "worker" && r.BaseName == BaseName), Is.True,
				"в метабазе должна появиться строка base_update_rights");
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
			var right = (await ReadBaseUpdateRights()).FirstOrDefault(r => r.Login == "worker");

			Assert.That(baseGrant, Is.Not.Null);
			Assert.That(baseGrant, Does.Contain("SELECT"));
			Assert.That(baseGrant, Does.Not.Contain("INSERT"), "на чтение - значит без записи");
			Assert.That(baseGrant, Does.Not.Contain("DELETE"));
			Assert.That(baseGrant, Does.Not.Contain("ALTER"), "без DDL миграцию не накатить");
			Assert.That(right?.CanUpdate, Is.False, "читателю базу обновлять нечем");
		}

		[Test(Description = "Администратор базы получает все права на неё и право её обновлять")]
		public async Task SetUserBaseAccess_BaseAdmin_GrantsAllPrivilegesAndUpdateRight() {
			provider.SetUserBaseAccess("worker", Access(isAdmin: true));

			var grants = await ReadServerGrants("worker");
			string baseGrant = FindGrantOnDatabase(grants, BaseName);
			var right = (await ReadBaseUpdateRights()).FirstOrDefault(r => r.Login == "worker");

			Assert.That(baseGrant, Does.Contain("ALL PRIVILEGES"));
			Assert.That(right?.CanUpdate, Is.True, "администратор базы вправе накатывать на неё обновления");
		}

		[Test(Description = "Обычный доступ к базе включает DDL - им и накатываются обновления")]
		public async Task SetUserBaseAccess_PlainUser_GrantsDdlForUpdates() {
			provider.SetUserBaseAccess("worker", Access());

			var grants = await ReadServerGrants("worker");
			string baseGrant = FindGrantOnDatabase(grants, BaseName);
			var right = (await ReadBaseUpdateRights()).FirstOrDefault(r => r.Login == "worker");

			Assert.That(baseGrant, Does.Contain("ALTER"));
			Assert.That(baseGrant, Does.Contain("CREATE"));
			Assert.That(baseGrant, Does.Contain("DROP"));
			Assert.That(right?.CanUpdate, Is.True,
				"право обновлять базу - это выданный набор с DDL, а не отдельная запись");
		}

		[Test(Description = "Снятие доступа убирает гранты, право на обновление и деактивирует пользователя в базе")]
		public async Task SetUserBaseAccess_Revoked_CleansUpEverywhere() {
			provider.SetUserBaseAccess("worker", Access());
			provider.SetUserBaseAccess("worker", Access(hasAccess: false)); // и сразу отбираем

			var grants = await ReadServerGrants("worker");
			var rights = await ReadBaseUpdateRights();
			var baseUser = await ReadBaseUser(BaseName, "worker");

			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.False,
				"грант на базу должен быть отозван");
			Assert.That(rights.Any(r => r.Login == "worker" && r.BaseName == BaseName), Is.False,
				"строка base_update_rights должна исчезнуть");
			Assert.That(baseUser, Is.Not.Null, "саму строку пользователя в базе не удаляем");
			Assert.That(baseUser?.Deactivated, Is.True, "доступ снимается флагом deactivated");
		}

		[Test(Description = "Повторная выдача доступа не плодит гранты и строки")]
		public async Task SetUserBaseAccess_AppliedTwice_IsIdempotent() {
			provider.SetUserBaseAccess("worker", Access());
			provider.SetUserBaseAccess("worker", Access()); // тот же доступ второй раз

			var rights = (await ReadBaseUpdateRights()).Where(r => r.Login == "worker").ToList();
			var baseUsers = (await ReadBaseUsers(BaseName)).Where(u => u.Login == "worker").ToList();

			Assert.That(rights, Has.Count.EqualTo(1), "в base_update_rights должна быть одна строка");
			Assert.That(baseUsers, Has.Count.EqualTo(1), "в users базы - тоже одна");
		}

		[Test(Description = "Список доступов показывает все базы продукта, а не только доступные")]
		public async Task GetUserBaseAccess_ListsAllProductBasesWithFlags() {
			await CreateApplicationDatabase("base_second", "Вторая"); // вторая база продукта, доступа на неё не будет
			await SeedMetabaseBase("base_second", "Вторая");

			provider.SetUserBaseAccess("worker", Access());

			var rows = provider.GetUserBaseAccess("worker");

			Assert.That(rows.Select(r => r.BaseName), Is.EquivalentTo(new[] { BaseName, "base_second" }));
			Assert.That(rows.First(r => r.BaseName == BaseName).HasAccess, Is.True);
			Assert.That(rows.First(r => r.BaseName == "base_second").HasAccess, Is.False);
		}

		[Test(Description = "Доступы вычисляются из реальных грантов сервера")]
		public async Task GetUserBaseAccess_ComputedFromGrants() {
			await GrantOnDatabase("worker", BaseName, "SELECT, LOCK TABLES, SHOW VIEW");

			var row = provider.GetUserBaseAccess("worker").FirstOrDefault(r => r.BaseName == BaseName);

			Assert.That(row, Is.Not.Null);
			Assert.That(row?.HasAccess, Is.True);
			Assert.That(row?.ReadOnly, Is.True, "только читающие привилегии - значит доступ на чтение");
			Assert.That(row?.IsAdmin, Is.False);
		}

		[Test(Description = "База без грантов показывается без доступа")]
		public async Task GetUserBaseAccess_BaseWithoutGrantsHasNoAccess() {
			await CreateApplicationDatabase("base_no_grants", "Без грантов"); // грантов на неё не выдаём
			await SeedMetabaseBase("base_no_grants", "Без грантов");
			await GrantOnDatabase("worker", BaseName, "SELECT, INSERT"); // а на соседнюю - выдаём

			var rows = provider.GetUserBaseAccess("worker");

			// у любой учётки есть GRANT USAGE ON *.*, и он не должен читаться как доступ
			Assert.That(rows.First(r => r.BaseName == "base_no_grants").HasAccess, Is.False,
				"USAGE - это отсутствие привилегий, а не доступ ко всем базам");
			Assert.That(rows.First(r => r.BaseName == BaseName).HasAccess, Is.True,
				"предусловие: там, где гранты есть, доступ виден");
		}

		[Test(Description = "У глобального администратора доступ ко всем базам и правится он не здесь")]
		public async Task GetUserBaseAccess_GlobalAdmin_ReportsFullAccessNotEditable() {
			await CreateServerLogin("superuser", "super-pass", isAdmin: true); // права на весь сервер

			var row = provider.GetUserBaseAccess("superuser").FirstOrDefault(r => r.BaseName == BaseName);

			Assert.That(row?.HasAccess, Is.True);
			Assert.That(row?.IsAdmin, Is.True);
			Assert.That(row?.CanEdit, Is.False,
				"права выданы на весь сервер - побазово их редактировать нельзя");
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

		[Test(Description = "Базы, которой ещё нет в каталоге, на экране доступов не будет")]
		public async Task GetUserBaseAccess_BaseNotInCatalog_NotListedUntilRefresh() {
			await CreateApplicationDatabase("base_unlisted", "Не в каталоге"); // в метабазу не заносим

			var before = provider.GetUserBaseAccess("worker").Select(r => r.BaseName).ToList();
			provider.RefreshMetadata();
			var after = provider.GetUserBaseAccess("worker").Select(r => r.BaseName).ToList();

			Assert.That(before, Does.Not.Contain("base_unlisted"),
				"список собирается из каталога метабазы одним запросом, а не обходом баз сервера");
			Assert.That(after, Does.Contain("base_unlisted"),
				"после синхронизации метаинформации база появляется");
		}

		[Test(Description = "Расхождение: право есть в метабазе, но грантов на сервере нет")]
		public async Task GetUserBaseAccess_RightInMetabaseWithoutGrants_ServerWins() {
			int workerId = (await ReadMetabaseUser("worker")).Id;
			// право проставлено только в метабазе, минуя сервер - расхождение собрано вручную
			await GrantBaseUpdateRight(workerId, baseId);

			var row = provider.GetUserBaseAccess("worker").FirstOrDefault(r => r.BaseName == BaseName);

			Assert.That(row?.HasAccess, Is.False,
				"доступ снимается с грантов сервера - метабаза его не хранит и переспорить не может");
		}
	}
}
