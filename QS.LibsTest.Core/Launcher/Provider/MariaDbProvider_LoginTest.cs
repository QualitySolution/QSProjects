using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	public class MariaDbProvider_LoginTest : LauncherDbTestFixtureBase
	{
		[Test(Description = "Права на создание и удаление считаются по реальным грантам")]
		public async Task LoginToServer_UserWithCreateGrant_CanCreateButNotManageUsers() {
			await CreateServerLogin("creator", "creator-pass");
			await GrantOnDatabase("creator", "%", "CREATE, DROP"); // права на базы, но не на сервер

			var provider = CreateProvider("creator", "creator-pass");
			provider.LoginToServer();

			Assert.That(provider.CanCreateDatabase, Is.True);
			Assert.That(provider.CanDropDatabase, Is.True);
			Assert.That(provider.IsAdmin, Is.False, "гранты на базы - не глобальный админ");
			Assert.That(provider.CanManageUsers, Is.False);
		}

		[Test(Description = "Конструктор не должен ходить в сеть — иначе интерфейс замирает на выборе подключения")]
		public void CreateProvider_UnreachableServer_ConstructorDoesNotBlock() {
			// адрес заведомо не существующий
			var parameters = new List<ConnectionParameterValue> {
				new ConnectionParameterValue(new ConnectionParameter("Server", "Сервер"), "203.0.113.1:3306"),
				new ConnectionParameterValue(new ConnectionParameter("Login", "Пользователь"), "someone")
			};

			var stopwatch = Stopwatch.StartNew();
			using(new MariaDBProvider(parameters, TestProductCode, "pass")) {
				stopwatch.Stop();
			}

			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
				"конструктор обязан только собрать строку подключения - никаких запросов к серверу");
		}

		[Test(Description = "Вход укладывается в разумное время на пустом сервере")]
		public void LoginToServer_Duration_IsWithinBudget() {
			var provider = CreateProvider();

			var stopwatch = Stopwatch.StartNew();
			var response = provider.LoginToServer();
			stopwatch.Stop();

			Assert.That(response.Success, Is.True);
			// бюджет с большим запасом: ловим не милисекунды, а появление лишних round-trip'ов
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000),
				$"вход занял {stopwatch.ElapsedMilliseconds} мс - похоже, добавились лишние запросы к серверу");
		}

		[Test(Description = "Повторный вход пересчитывает права, а не отдаёт закешированные")]
		public async Task LoginToServer_AfterGrantChanged_RecalculatesRights() {
			await CreateServerLogin("grower", "grower-pass");

			var provider = CreateProvider("grower", "grower-pass");
			provider.LoginToServer();
			Assert.That(provider.CanCreateDatabase, Is.False, "предусловие: прав ещё нет");

			await GrantOnDatabase("grower", "%", "CREATE"); // выдаём уже после первого входа

			provider.LoginToServer();

			Assert.That(provider.CanCreateDatabase, Is.True, "после выдачи гранта повторный вход должен это увидеть");
		}

		[Test(Description = "Без метабазы вход проходит нормально - она необязательна")]
		public async Task LoginToServer_WithoutMetabase_Succeeds() {
			await DropMetabase(); // работаем на сервере вообще без QSLauncher
			try {
				var provider = CreateProvider();

				var response = provider.LoginToServer();

				Assert.That(response.Success, Is.True, response.ErrorMessage);
				Assert.That(provider.IsAdmin, Is.True);
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Заблокированная учётка на сервер не пускает")]
		public async Task LoginToServer_LockedAccount_Fails()
		{
			await CreateServerLogin("locked", "locked-pass", locked: true); // сразу с ACCOUNT LOCK

			var provider = CreateProvider("locked", "locked-pass");
			var response = provider.LoginToServer();

			Assert.That(response.Success, Is.False, "заблокированная учётка не должна входить");
		}
	}
}
