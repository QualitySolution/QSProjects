using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Пользователи: учётка на сервере, запись в метабазе и строка в users каждой базы
	/// должны меняться согласованно.
	/// </summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_UsersTest : LauncherDbTestFixtureBase {

		/// <summary>
		/// Есть ли среди грантов права на весь сервер - признак администратора.
		/// Своим методом, а не Contains: в netstandard2.0 у string нет перегрузки со StringComparison.
		/// </summary>
		private static bool HasGlobalPrivileges(IEnumerable<string> grants) =>
			grants.Any(g => g.IndexOf("ALL PRIVILEGES ON *.*", StringComparison.OrdinalIgnoreCase) >= 0);

		[Test(Description = "Созданный пользователь заводится на сервере под обоими хостами и попадает в метабазу")]
		public async Task CreateUser_CreatesServerAccountsAndMetabaseRecord() {
			var provider = LoginAs();

			bool created = provider.CreateUser(new DbUserInfo {
				Login = "newbie", Name = "Новичок", Email = "newbie@example.com"
			}, "newbie-pass");

			// три места, где пользователь должен появиться: сервер, метабаза, а профиль - в users базы
			var hosts = await ReadServerLoginHosts("newbie");
			var metabaseRow = await ReadMetabaseUser("newbie");

			Assert.That(created, Is.True);
			Assert.That(hosts, Is.EquivalentTo(new[] { "%", "localhost" }),
				"учётка заводится и для удалённых подключений, и для локальных");
			Assert.That(metabaseRow, Is.Not.Null, "пользователь должен отразиться в метабазе");
			Assert.That(metabaseRow?.Name, Is.EqualTo("Новичок"));
			Assert.That(metabaseRow?.Email, Is.EqualTo("newbie@example.com"));
			Assert.That(metabaseRow?.PasswordHash, Is.Not.Null.And.Not.EqualTo("newbie-pass"),
				"пароль в метабазе хранится хешем");
		}

		[Test(Description = "Созданный пользователь может войти на сервер")]
		public void CreateUser_NewUserCanLogIn() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "loginable" }, "loginable-pass");

			var asNewUser = CreateProvider("loginable", "loginable-pass");
			var response = asNewUser.LoginToServer();

			Assert.That(response.Success, Is.True, response.ErrorMessage);
		}

		[Test(Description = "Новому пользователю выдаётся чтение метабазы - иначе он не увидит список баз")]
		public async Task CreateUser_GrantsReadAccessToMetabase() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "reader" }, "reader-pass");

			var grants = await ReadServerGrants("reader");

			Assert.That(GrantsMentionDatabase(grants, LauncherDbName), Is.True,
				"без чтения QSLauncher новый пользователь не увидит ни одной базы");
		}

		[Test(Description = "Пользователь-администратор получает глобальные права")]
		public async Task CreateUser_AdminFlag_GrantsGlobalPrivileges() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "chief", IsAdmin = true }, "chief-pass"); // сразу админом

			var grants = await ReadServerGrants("chief");

			Assert.That(HasGlobalPrivileges(grants), Is.True,
				"администратору выдаются права на весь сервер");
		}

		[Test(Description = "Пустой пароль и пустой логин отвергаются до обращения к серверу")]
		public void CreateUser_InvalidArguments_Throws() {
			var provider = LoginAs();

			Assert.Throws<ArgumentException>(
				() => provider.CreateUser(new DbUserInfo { Login = "someone" }, string.Empty));
			Assert.Throws<ArgumentException>(
				() => provider.CreateUser(new DbUserInfo { Login = "   " }, "pass"));
		}

		[Test(Description = "Смена пароля пользователя пускает его по новому паролю")]
		public void UpdateUser_NewPassword_TakesEffect() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "repass" }, "old-pass");

			provider.UpdateUser(new DbUserInfo { Login = "repass" }, "new-pass");

			Assert.That(CreateProvider("repass", "new-pass").LoginToServer().Success, Is.True,
				"новый пароль должен работать");
			Assert.That(CreateProvider("repass", "old-pass").LoginToServer().Success, Is.False,
				"старый пароль должен перестать работать");
		}

		[Test(Description = "Блокировка пользователя закрывает ему вход")]
		public void UpdateUser_Disabled_BlocksLogin() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "blockme" }, "blockme-pass");

			provider.UpdateUser(new DbUserInfo {
				Login = "blockme", Disabled = true
			});

			Assert.That(CreateProvider("blockme", "blockme-pass").LoginToServer().Success, Is.False,
				"заблокированная учётка входить не должна");
		}

		[Test(Description = "Снятие блокировки возвращает вход")]
		public void UpdateUser_Enabled_RestoresLogin() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "unblockme", Disabled = true }, "unblock-pass"); // создан заблокированным

			provider.UpdateUser(new DbUserInfo {
				Login = "unblockme", Disabled = false
			});

			Assert.That(CreateProvider("unblockme", "unblock-pass").LoginToServer().Success, Is.True);
		}

		[Test(Description = "Выдача и снятие флага администратора меняет глобальные гранты")]
		public async Task UpdateUser_AdminFlag_TogglesGlobalGrants() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "promoted" }, "promoted-pass");

			provider.UpdateUser(new DbUserInfo {
				Login = "promoted", IsAdmin = true
			});
			var afterPromotion = await ReadServerGrants("promoted"); // сняли гранты после повышения

			provider.UpdateUser(new DbUserInfo {
				Login = "promoted", IsAdmin = false
			});
			var afterDemotion = await ReadServerGrants("promoted"); // и после понижения

			Assert.That(HasGlobalPrivileges(afterPromotion),
				Is.True, "после повышения должны появиться глобальные права");
			Assert.That(HasGlobalPrivileges(afterDemotion),
				Is.False, "после понижения глобальные права должны уйти");
		}

		[Test(Description = "Правка профиля без изменения учётки отражается в метабазе")]
		public async Task UpdateUser_Profile_ReflectedInMetabase() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "profiled", Name = "Было" }, "profiled-pass");

			provider.UpdateUser(new DbUserInfo {
				Login = "profiled", Name = "Стало", Email = "stalo@example.com"
			});

			var row = await ReadMetabaseUser("profiled");
			Assert.That(row?.Name, Is.EqualTo("Стало"));
			Assert.That(row?.Email, Is.EqualTo("stalo@example.com"));
		}

		[Test(Description = "Удаление пользователя убирает учётки со всех хостов и запись из метабазы")]
		public async Task DeleteUser_RemovesServerAccountsAndMetabaseRecord() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "condemned" }, "condemned-pass");

			bool deleted = provider.DeleteUser("condemned");

			bool existsOnServer = await ServerLoginExists("condemned");
			var metabaseRow = await ReadMetabaseUser("condemned");

			Assert.That(deleted, Is.True);
			Assert.That(existsOnServer, Is.False, "учётки должны исчезнуть по всем хостам");
			Assert.That(metabaseRow, Is.Null, "явное удаление из метабазы жёсткое, а не мягкое");
		}

		[Test(Description = "Удаление пользователя деактивирует его строку в users каждой базы продукта")]
		public async Task DeleteUser_DeactivatesRowsInApplicationDatabases() {
			await CreateApplicationDatabase("base_alpha");
			await CreateApplicationDatabase("base_beta");

			var provider = LoginAs();
			int userId = (await ReadMetabaseUser(RootLogin)).Id;
			int alphaId = await SeedMetabaseBase("base_alpha");
			int betaId = await SeedMetabaseBase("base_beta");
			await GrantMetabaseAccess(userId, alphaId);
			await GrantMetabaseAccess(userId, betaId); // обе базы видны администратору

			provider.CreateUser(new DbUserInfo { Login = "wanderer", Name = "Странник" }, "wanderer-pass");
			provider.SetUserBaseAccess("wanderer",
				new DbUserBaseAccess { BaseName = "base_alpha", BaseId = alphaId, HasAccess = true, Name = "Странник" });
			provider.SetUserBaseAccess("wanderer",
				new DbUserBaseAccess { BaseName = "base_beta", BaseId = betaId, HasAccess = true, Name = "Странник" });

			provider.DeleteUser("wanderer");

			// проверяем обе базы: обход не должен обрываться на первой
			var inAlpha = await ReadBaseUser("base_alpha", "wanderer");
			var inBeta = await ReadBaseUser("base_beta", "wanderer");

			Assert.That(inAlpha, Is.Not.Null, "строку в базе не удаляем - помечаем");
			Assert.That(inAlpha?.Deactivated, Is.True, "в base_alpha пользователь должен быть отключён");
			Assert.That(inBeta, Is.Not.Null);
			Assert.That(inBeta?.Deactivated, Is.True, "во второй базе тоже - обход не должен обрываться на первой");
		}

		[Test(Description = "Список пользователей приходит из метабазы")]
		public async Task GetUsers_FromMetabase_ReturnsAccountUsers() {
			await SeedMetabaseUser("meta_only_user", name: "Только в метабазе"); // учётки на сервере нет

			var provider = LoginAs();
			var users = provider.GetUsers();

			var found = users.FirstOrDefault(u => u.Login == "meta_only_user");
			Assert.That(found, Is.Not.Null, "пользователей показываем из метабазы");
			Assert.That(found?.Name, Is.EqualTo("Только в метабазе"));
		}

		[Test(Description = "Без метабазы список пользователей собирается с сервера без служебных учёток")]
		public async Task GetUsers_WithoutMetabase_ReturnsRealAccountsOnly() {
			await DropMetabase();
			try {
				await CreateServerLogin("real_user", "real-pass");

				var provider = LoginAs();
				var logins = provider.GetUsers().Select(u => u.Login).ToList();

				Assert.That(logins, Does.Contain("real_user"));
				Assert.That(logins, Does.Not.Contain("root"), "служебные учётки сервера пользователю не показываем");
				Assert.That(logins.Any(l => l.StartsWith("mysql.", StringComparison.OrdinalIgnoreCase)), Is.False);
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Обычный пользователь меняет себе пароль без прав администратора")]
		public void ChangeOwnPassword_PlainUser_ChangesPasswordOnServer() {
			var admin = LoginAs();
			admin.CreateUser(new DbUserInfo { Login = "selfchanger" }, "first-pass");

			var self = LoginAs("selfchanger", "first-pass");
			bool changed = self.ChangeOwnPassword("second-pass");

			Assert.That(changed, Is.True);
			Assert.That(CreateProvider("selfchanger", "second-pass").LoginToServer().Success, Is.True,
				"новый пароль должен пускать на сервер");
			Assert.That(CreateProvider("selfchanger", "first-pass").LoginToServer().Success, Is.False,
				"старый пароль должен перестать работать");
		}

		[Test(Description = "После смены своего пароля провайдер продолжает открывать новые соединения")]
		public void ChangeOwnPassword_KeepsProviderUsable() {
			var admin = LoginAs();
			admin.CreateUser(new DbUserInfo { Login = "stillworking" }, "first-pass");

			var self = LoginAs("stillworking", "first-pass");
			self.ChangeOwnPassword("second-pass");

			// список баз открывает подключения помимо основного - со старым паролем они получат отказ
			Assert.DoesNotThrow(() => self.GetUserDatabases(),
				"строка подключения провайдера должна была обновиться вместе с паролем");
		}

		[Test(Description = "Администратору смена своего пароля обновляет и хеш в метабазе")]
		public async Task ChangeOwnPassword_Admin_UpdatesMetabaseHash() {
			var admin = LoginAs();
			admin.CreateUser(new DbUserInfo { Login = "selfadmin", IsAdmin = true }, "first-pass"); // админу метабаза доступна на запись

			var self = LoginAs("selfadmin", "first-pass");
			string hashBefore = (await ReadMetabaseUser("selfadmin"))?.PasswordHash;

			bool changed = self.ChangeOwnPassword("second-pass");

			string hashAfter = (await ReadMetabaseUser("selfadmin"))?.PasswordHash;
			Assert.That(changed, Is.True);
			Assert.That(CreateProvider("selfadmin", "second-pass").LoginToServer().Success, Is.True,
				"новый пароль должен пускать на сервер");
			Assert.That(hashAfter, Is.Not.EqualTo(hashBefore), "хеш в метабазе должен обновиться");
		}

		[Test(Description = "Операции с пользователями работают и без метабазы")]
		public async Task UserLifecycle_WithoutMetabase_WorksOnServerOnly() {
			await DropMetabase();
			try {
				var provider = LoginAs();

				Assert.DoesNotThrow(() => provider.CreateUser(new DbUserInfo { Login = "lonely" }, "lonely-pass"),
					"метабаза необязательна - создание пользователя обязано пройти");
				Assert.That(await ServerLoginExists("lonely"), Is.True);

				Assert.DoesNotThrow(() => provider.DeleteUser("lonely"));
				Assert.That(await ServerLoginExists("lonely"), Is.False);
			}
			finally {
				await DeployMetabase();
			}
		}
	}
}
