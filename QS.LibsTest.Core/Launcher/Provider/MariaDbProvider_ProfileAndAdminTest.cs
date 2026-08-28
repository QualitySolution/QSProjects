using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_ProfileAndAdminTest : LauncherDbTestFixtureBase {

		private static bool HasGlobalPrivileges(IEnumerable<string> grants) =>
			grants.Any(g => g.IndexOf("ALL PRIVILEGES ON *.*", StringComparison.OrdinalIgnoreCase) >= 0);

		private async Task<MariaDBProvider> WithUserInTwoBases(string login)
		{
			await CreateApplicationDatabase("profile_alpha");
			await CreateApplicationDatabase("profile_beta");

			var provider = LoginAs();
			int alphaId = await SeedMetabase("profile_alpha");
			int betaId = await SeedMetabase("profile_beta");

			provider.CreateUser(new DbUserInfo { Login = login, Name = "Было" }, login + "-pass");
			provider.SetUserBaseAccess(login,
				new DbUserBaseAccess { BaseName = "profile_alpha", BaseId = alphaId, HasAccess = true });
			provider.SetUserBaseAccess(login,
				new DbUserBaseAccess { BaseName = "profile_beta", BaseId = betaId, HasAccess = true });
			return provider;
		}

		[Test(Description = "Правка одного имени доезжает до users всех баз, хотя доступы не трогали")]
		public async Task UpdateUser_ProfileOnly_ReachesAllBases() {
			var provider = await WithUserInTwoBases("profiled");

			// ни одного вызова SetUserBaseAccess: раньше в этом случае имя не записывалось никуда
			provider.UpdateUser(new DbUserInfo {
				Login = "profiled", Name = "Стало", Email = "stalo@example.com"
			});

			var inAlpha = await ReadBaseUser("profile_alpha", "profiled");
			var inBeta = await ReadBaseUser("profile_beta", "profiled");

			Assert.That(inAlpha?.Name, Is.EqualTo("Стало"));
			Assert.That(inAlpha?.Email, Is.EqualTo("stalo@example.com"));
			Assert.That(inBeta?.Name, Is.EqualTo("Стало"), "во вторую базу тоже");
			Assert.That(inBeta?.Email, Is.EqualTo("stalo@example.com"), "обход не обрывается на первой");
		}

		[Test(Description = "Пустые поля профиля не затирают то, что уже записано в базе")]
		public async Task UpdateUser_EmptyProfileFields_KeepStoredValues() {
			var provider = await WithUserInTwoBases("keepname");
			provider.UpdateUser(new DbUserInfo { Login = "keepname", Name = "Имя", Email = "mail@example.com" });

			provider.UpdateUser(new DbUserInfo { Login = "keepname", Name = "Только имя" });

			var row = await ReadBaseUser("profile_alpha", "keepname");
			Assert.That(row?.Name, Is.EqualTo("Только имя"));
			Assert.That(row?.Email, Is.EqualTo("mail@example.com"), "пустая почта не должна стирать прежнюю");
		}

		[Test(Description = "Правка профиля не задевает признак администратора базы")]
		public async Task UpdateUser_ProfileOnly_KeepsBaseAdminFlag() {
			var provider = await WithUserInTwoBases("baseadmin");
			int alphaId = (await ReadMetabaseBases()).First(b => b.BaseName == "profile_alpha").Id;
			provider.SetUserBaseAccess("baseadmin",
				new DbUserBaseAccess { BaseName = "profile_alpha", BaseId = alphaId, HasAccess = true, IsAdmin = true });

			provider.UpdateUser(new DbUserInfo { Login = "baseadmin", Name = "Переименован" });

			var row = await ReadBaseUser("profile_alpha", "baseadmin");
			Assert.That(row?.Name, Is.EqualTo("Переименован"));
			Assert.That(row?.Admin, Is.True, "admin в users не должен сбросываться");
		}

		[Test(Description = "Повторное сохранение с тем же флагом ничего не ломает")]
		public async Task UpdateUser_AdminFlag_SavedTwice_StaysAdmin() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "stillchief", IsAdmin = true }, "chief-pass");

			// желаемое совпадает с фактическим - провайдер не должен выдавать грант повторно или падать
			Assert.DoesNotThrow(() => provider.UpdateUser(new DbUserInfo { Login = "stillchief", IsAdmin = true }));
			Assert.That(HasGlobalPrivileges(await ReadServerGrants("stillchief")), Is.True);
		}

		[Test(Description = "Понижение обычного пользователя не ошибается, хотя снимать нечего")]
		public async Task UpdateUser_DemoteNonAdmin_DoesNotFail() {
			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "neveradmin" }, "never-pass");

			Assert.DoesNotThrow(() => provider.UpdateUser(new DbUserInfo { Login = "neveradmin", IsAdmin = false }));

			Assert.That(HasGlobalPrivileges(await ReadServerGrants("neveradmin")), Is.False);
		}

		[Test(Description = "Снятие админа не забирает права, выданные на отдельные базы")]
		public async Task UpdateUser_Demote_KeepsDatabaseGrants() {
			var provider = await WithUserInTwoBases("demoted");
			provider.UpdateUser(new DbUserInfo { Login = "demoted", IsAdmin = true });

			provider.UpdateUser(new DbUserInfo { Login = "demoted", IsAdmin = false });

			var grants = await ReadServerGrants("demoted");
			Assert.That(HasGlobalPrivileges(grants), Is.False, "глобальные права должны уйти");
			Assert.That(GrantsMentionDatabase(grants, "profile_alpha"), Is.True,
				"REVOKE ... ON *.* не должен задевать грантов уровня базы");
		}
	}
}
