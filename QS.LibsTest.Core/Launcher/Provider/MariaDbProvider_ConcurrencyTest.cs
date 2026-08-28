using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Provider {
	/// <summary>Гонки</summary>
	[TestFixture(TestOf = typeof(MariaDBProvider))]
	[Category("Concurrency")]
	public class MariaDbProvider_ConcurrencyTest : LauncherDbTestFixtureBase
	{
		private async Task SeedTwoBases() {
			await CreateApplicationDatabase("base_conc_a", "А");
			await CreateApplicationDatabase("base_conc_b", "Б");
		}

		[Test(Description = "Одновременные чтения с сервера напрямую не путают кеш хостов учёток")]
		public async Task ParallelDirectReads_DoNotCorruptUserHostsCache() {
			await DropMetabase(); // без метабазы всё пойдёт через одно соединение
			try {
				for(int i = 0; i < 10; i++)
					await CreateServerLogin($"host_race_{i:D2}", "pass");

				var provider = LoginAs();
				var errors = new ConcurrentBag<Exception>();

				// GetUsers чистит и заполняет словарь хостов, GetUserBaseAccess его читает
				var tasks = new List<Task>();
				for(int i = 0; i < 10; i++) {
					int index = i;
					tasks.Add(Task.Run(() => {
						try { provider.GetUsers(); }
						catch(Exception ex) { errors.Add(ex); }
					}));
					tasks.Add(Task.Run(() => {
						try { provider.GetUserBaseAccess($"host_race_{index:D2}"); }
						catch(Exception ex) { errors.Add(ex); }
					}));
				}
				await Task.WhenAll(tasks);

				Assert.That(errors, Is.Empty,
					"кеш хостов - обычный Dictionary: он чистится в GetUsersDirect и читается в HostsOf. "
					+ "Первая ошибка: " + (errors.FirstOrDefault()?.Message ?? "нет"));
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Синхронизация метабазы параллельно с удалением базы не оставляет мусора")]
		public async Task RefreshMetadataDuringDrop_LeavesConsistentMetabase() {
			var provider = LoginAs();
			await SeedTwoBases();

			var errors = new ConcurrentBag<Exception>();

			var refresh = Task.Run(() => {
				try { provider.RefreshMetadata(); }
				catch(Exception ex) { errors.Add(ex); }
			});
			var drop = Task.Run(() => {
				try { provider.DropDatabase(new DbInfo { BaseName = "base_conc_b" }); }
				catch(Exception ex) { errors.Add(ex); }
			});
			await Task.WhenAll(refresh, drop);

			bool stillOnServer = await DatabaseExists("base_conc_b");
			var record = await ReadMetabaseBase("base_conc_b");

			Assert.That(errors, Is.Empty,
				"обе операции идут через одно соединение провайдера. Первая ошибка: "
				+ (errors.FirstOrDefault()?.Message ?? "нет"));
			Assert.That(stillOnServer, Is.False, "база должна быть удалена");
			// синхронизация могла успеть вписать базу обратно - тогда она обязана быть disabled,
			// а не числиться живой
			Assert.That(record == null || record.Disabled, Is.True,
				"после удаления база не должна остаться в метабазе активной");
		}

		[Test(Description = "Одновременная выдача доступов разным пользователям не теряет ни одного")]
		public async Task ParallelAccessGrants_AllApplied() {
			await CreateApplicationDatabase("base_parallel_access", "Общая");
			int baseId = await SeedMetabase("base_parallel_access", "Общая");

			var provider = LoginAs();
			const int userCount = 6;
			for(int i = 0; i < userCount; i++)
				provider.CreateUser(new DbUserInfo { Login = $"grantee_{i}", Name = $"Пользователь {i}" }, "pass-1234"); // готовим по очереди

			var errors = new ConcurrentBag<Exception>();
			var tasks = Enumerable.Range(0, userCount).Select(i => Task.Run(() => {
				try {
					provider.SetUserBaseAccess($"grantee_{i}", new DbUserBaseAccess {
						BaseName = "base_parallel_access", BaseId = baseId, HasAccess = true, Name = $"Пользователь {i}"
					});
				}
				catch(Exception ex) { errors.Add(ex); }
			})).ToList();
			await Task.WhenAll(tasks);

			var baseUsers = await ReadBaseUsers("base_parallel_access");

			Assert.That(errors, Is.Empty,
				"выдача доступов из нескольких потоков не должна рвать соединение. Первая ошибка: "
				+ (errors.FirstOrDefault()?.Message ?? "нет"));
			Assert.That(baseUsers.Count(u => u.Login.StartsWith("grantee_", StringComparison.Ordinal)),
				Is.EqualTo(userCount), "ни один доступ не должен потеряться");
		}

		[Test(Description = "Метабаза собирается один раз, даже если к ней обратились сразу из нескольких потоков")]
		public async Task ParallelFirstAccess_BuildsMetadataConsistently() {
			await SeedMetabaseUser("lazy_meta_user");
			var provider = LoginAs();

			var errors = new ConcurrentBag<Exception>();
			var results = new ConcurrentBag<int>();
			var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() => {
				try { results.Add(provider.GetUsers().Count); }
				catch(Exception ex) { errors.Add(ex); }
			})).ToList();
			await Task.WhenAll(tasks);

			Assert.That(errors, Is.Empty,
				"ленивое создание метабазы не потокобезопасно по построению. Первая ошибка: "
				+ (errors.FirstOrDefault()?.Message ?? "нет"));
			Assert.That(results.Distinct().Count(), Is.LessThanOrEqualTo(1),
				"все потоки должны увидеть одинаковый список пользователей");
		}

		[Test(Description = "Два провайдера на одном сервере друг другу не мешают - у каждого своё соединение")]
		public async Task TwoProviders_WorkIndependently() {
			await CreateApplicationDatabase("base_shared_server");

			var first = LoginAs();
			var second = LoginAs();

			var errors = new ConcurrentBag<Exception>();
			var tasks = new List<Task>();
			for(int i = 0; i < 10; i++) {
				tasks.Add(Task.Run(() => {
					try { first.GetUserDatabases(); }
					catch(Exception ex) { errors.Add(ex); }
				}));
				tasks.Add(Task.Run(() => {
					try { second.GetUsers(); }
					catch(Exception ex) { errors.Add(ex); }
				}));
			}
			await Task.WhenAll(tasks);

			Assert.That(errors, Is.Empty,
				"разные провайдеры не разделяют состояние - здесь гонки быть не должно ни при каком раскладе. "
				+ "Ошибка тут означает проблему уровнем ниже. Первая: "
				+ (errors.FirstOrDefault()?.Message ?? "нет"));
		}
	}
}
