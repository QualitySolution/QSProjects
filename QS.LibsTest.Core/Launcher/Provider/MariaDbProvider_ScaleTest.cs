using Dapper;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>
	/// Поведение на объёме. Цель не измерить миллисекунды, а поймать переход к запросу на строку
	/// и другие обвалы на порядок.
	///
	/// Пороги стоят примерно в полсотни раз выше замеренного на тёплом контейнере (6-30 мс), чтобы
	/// пережить загруженный CI, и при этом сильно ниже стоимости регресса: возврат к подключению
	/// на каждую базу давал секунды.
	/// </summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	[Category("Performance")]
	public class MariaDbProvider_ScaleTest : LauncherDbTestFixtureBase {

		/// <summary>Быстро набивает метабазу пользователями одним запросом.</summary>
		private async Task SeedManyMetabaseUsers(int count) {
			var values = string.Join(",", Enumerable.Range(0, count)
				.Select(i => $"({TestAccountId}, {TestProductCode}, 'bulk_user_{i:D4}', 'Пользователь {i}')"));

			using(var connection = CreateConnection(LauncherDbName)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync(
					$"INSERT INTO `server_users` (account_id, product_id, login, name) VALUES {values};");
			}
		}

		[Test(Description = "Список из нескольких сотен пользователей читается одним запросом")]
		public async Task GetUsers_ManyUsersInMetabase_StaysFast() {
			const int userCount = 500;
			await SeedManyMetabaseUsers(userCount); // одним INSERT, чтобы подготовка не мешала замеру

			var provider = LoginAs();

			var stopwatch = Stopwatch.StartNew();
			var users = provider.GetUsers();
			stopwatch.Stop();

			Assert.That(users.Count, Is.GreaterThanOrEqualTo(userCount));
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
				$"{userCount} пользователей прочитались за {stopwatch.ElapsedMilliseconds} мс - "
				+ "это должен быть один SELECT");
		}

		[Test(Description = "Повторное чтение большого списка не деградирует от вызова к вызову")]
		public async Task GetUsers_RepeatedCalls_DoNotDegrade() {
			await SeedManyMetabaseUsers(300);
			var provider = LoginAs();

			provider.GetUsers(); // прогрев: соединение и метабаза уже подняты

			var first = Stopwatch.StartNew();
			provider.GetUsers();
			first.Stop();

			var later = Stopwatch.StartNew();
			for(int i = 0; i < 5; i++)
				provider.GetUsers();
			later.Stop();

			double averageLater = later.ElapsedMilliseconds / 5.0;
			Assert.That(averageLater, Is.LessThan(Math.Max(first.ElapsedMilliseconds * 3.0, 500)),
				$"первый вызов {first.ElapsedMilliseconds} мс, средний последующий {averageLater} мс - "
				+ "накопление состояния между вызовами");
		}

		[Test(Description = "Прямое чтение сотни учёток сервера остаётся одним запросом к mysql.user")]
		public async Task GetUsers_ManyServerAccounts_StaysFast() {
			await DropMetabase();
			try {
				const int accountCount = 100; // учётки заводим на сервере, а не в метабазе
				for(int i = 0; i < accountCount; i++)
					await CreateServerLogin($"srv_user_{i:D3}", "pass-1234");

				var provider = LoginAs();

				var stopwatch = Stopwatch.StartNew();
				var users = provider.GetUsers();
				stopwatch.Stop();

				Assert.That(users.Count(u => u.Login.StartsWith("srv_user_", StringComparison.Ordinal)),
					Is.EqualTo(accountCount));
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
					$"{accountCount} учёток прочитались за {stopwatch.ElapsedMilliseconds} мс");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Доступы по многим базам: профили читаются одним запросом на все базы")]
		public async Task GetUserBaseAccess_ManyBases_ProfileLookupDoesNotExplode() {
			const int baseCount = 25; // профиль есть в каждой из них - есть чему разъехаться на N запросов

			var provider = LoginAs();
			provider.CreateUser(new DbUserInfo { Login = "spread", Name = "Раскиданный" }, "spread-pass");

			for(int i = 0; i < baseCount; i++) {
				string baseName = $"base_spread_{i:D2}";
				await CreateApplicationDatabase(baseName, $"База {i}");
				int id = await SeedMetabaseBase(baseName, $"База {i}");
				provider.SetUserBaseAccess("spread", new DbUserBaseAccess {
					BaseName = baseName, BaseId = id, HasAccess = true, Name = "Раскиданный"
				});
			}

			var stopwatch = Stopwatch.StartNew();
			var rows = provider.GetUserBaseAccess("spread");
			stopwatch.Stop();

			Assert.That(rows.Count, Is.EqualTo(baseCount));
			// порог ловит возврат к чтению профиля отдельным запросом на каждую базу
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1500),
				$"доступы по {baseCount} базам собрались за {stopwatch.ElapsedMilliseconds} мс - "
				+ "профили должны читаться одним запросом на все базы");
		}

		[Test(Description = "Список баз не замедляется от числа баз в метабазе")]
		public async Task GetUserDatabases_ManyBases_StaysSingleQuery() {
			const int baseCount = 60; // только записи в метабазе, самих баз на сервере нет
			int rootId = (await ReadMetabaseUser(RootLogin)).Id;

			for(int i = 0; i < baseCount; i++) {
				int id = await SeedMetabaseBase($"base_many_{i:D3}", $"База {i}");
				await GrantMetabaseAccess(rootId, id);
			}

			var provider = LoginAs();

			var stopwatch = Stopwatch.StartNew();
			var databases = provider.GetUserDatabases();
			stopwatch.Stop();

			Assert.That(databases.Count, Is.EqualTo(baseCount));
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
				$"{baseCount} баз из метабазы прочитались за {stopwatch.ElapsedMilliseconds} мс - "
				+ "чтение из метабазы должно быть одним запросом");
		}

		[Test(Description = "Прямой список баз читает base_parameters всех баз одним запросом")]
		public async Task GetUserDatabases_DirectWithManyBases_HasKnownCost() {
			await DropMetabase();
			try {
				const int baseCount = 20; // настоящие базы: у каждой свои base_parameters
				for(int i = 0; i < baseCount; i++)
					await CreateApplicationDatabase($"base_direct_{i:D2}", $"База {i}");

				var provider = LoginAs();

				var stopwatch = Stopwatch.StartNew();
				var databases = provider.GetUserDatabases();
				stopwatch.Stop();

				Assert.That(databases.Count, Is.EqualTo(baseCount));
				// порог ловит возврат к подключению на каждую базу - на сервере с DNS-резолвом
				// это стоило бы секунд десять на каждую
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
					$"прямое чтение {baseCount} баз заняло {stopwatch.ElapsedMilliseconds} мс");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Создание пользователей подряд не накапливает стоимость")]
		public void CreateUser_Sequentially_CostPerUserIsStable() {
			var provider = LoginAs();

			var firstBatch = Stopwatch.StartNew();
			for(int i = 0; i < 10; i++)
				provider.CreateUser(new DbUserInfo { Login = $"early_{i:D2}" }, "pass-1234");
			firstBatch.Stop();

			var lastBatch = Stopwatch.StartNew(); // та же работа, но когда пользователей уже вдвое больше
			for(int i = 0; i < 10; i++)
				provider.CreateUser(new DbUserInfo { Login = $"late_{i:D2}" }, "pass-1234");
			lastBatch.Stop();

			Assert.That(lastBatch.ElapsedMilliseconds, Is.LessThan(Math.Max(firstBatch.ElapsedMilliseconds * 3, 5000)),
				$"первая десятка {firstBatch.ElapsedMilliseconds} мс, последняя {lastBatch.ElapsedMilliseconds} мс - "
				+ "стоимость создания не должна расти с числом уже созданных");
		}
	}
}
