using Grpc.Core;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Доступ к базам и поведение на краях. У облака доступ живёт в одном месте, зато появляется
	/// свой набор краёв: обрывы связи, отказы сервиса, чужие коды ошибок.
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_BaseAccessTest : CloudProviderTestFixtureBase {
		private const string BaseName = "access_base";

		private QSCloudProvider provider;
		private int baseId;

		[SetUp]
		public void SetUpScenario() {
			baseId = AddBase(BaseName, "Тестовая база").Id;
			AddUser("worker", "worker-pass", name: "Работник");
			provider = LoginAs();
		}

		[Test(Description = "Выдача доступа отражается в реестре облака")]
		public void SetUserBaseAccess_Granted_StoredInCloud() {
			provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseId = baseId, BaseName = BaseName, HasAccess = true
			});

			var access = State.FindAccess("worker", baseId);
			Assert.That(access, Is.Not.Null);
			Assert.That(access.HasAccess, Is.True);
			Assert.That(access.Admin, Is.False);
			Assert.That(access.ReadOnly, Is.False);
		}

		[Test(Description = "Доступ только на чтение сохраняется отдельным флагом")]
		public void SetUserBaseAccess_ReadOnly_StoredAsFlag() {
			provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseId = baseId, BaseName = BaseName, HasAccess = true, ReadOnly = true
			});

			var access = State.FindAccess("worker", baseId);
			Assert.That(access.ReadOnly, Is.True);
			Assert.That(access.Admin, Is.False);
		}

		[Test(Description = "Администратор базы сохраняется флагом admin")]
		public void SetUserBaseAccess_BaseAdmin_StoredAsFlag() {
			provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseId = baseId, BaseName = BaseName, HasAccess = true, IsAdmin = true
			});

			Assert.That(State.FindAccess("worker", baseId).Admin, Is.True);
		}

		[Test(Description = "Снятие доступа убирает строку из реестра")]
		public void SetUserBaseAccess_Revoked_RemovesRow() {
			Grant("worker", baseId);

			provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
				BaseId = baseId, BaseName = BaseName, HasAccess = false // снимаем
			});

			Assert.That(State.FindAccess("worker", baseId), Is.Null);
		}

		[Test(Description = "Повторная выдача доступа не плодит строк")]
		public void SetUserBaseAccess_AppliedTwice_IsIdempotent() {
			var access = new DbUserBaseAccess { BaseId = baseId, BaseName = BaseName, HasAccess = true };
			provider.SetUserBaseAccess("worker", access);
			provider.SetUserBaseAccess("worker", access);

			Assert.That(State.Access.Count(a => a.Login == "worker" && a.BaseId == baseId), Is.EqualTo(1));
		}

		[Test(Description = "Список доступов показывает все базы продукта с флагами")]
		public void GetUserBaseAccess_ListsAllProductBasesWithFlags() {
			int secondId = AddBase("second_base", "Вторая").Id;
			AddBase("alien_base", product: OtherProductCode); // чужой продукт в список не идёт
			Grant("worker", baseId, readOnly: true);

			var rows = provider.GetUserBaseAccess("worker");

			Assert.That(rows.Select(r => r.BaseId), Is.EquivalentTo(new[] { baseId, secondId }));
			Assert.That(rows.First(r => r.BaseId == baseId).HasAccess, Is.True);
			Assert.That(rows.First(r => r.BaseId == baseId).ReadOnly, Is.True);
			Assert.That(rows.First(r => r.BaseId == secondId).HasAccess, Is.False);
		}

		[Test(Description = "Доступ несуществующему пользователю - отказ облака с объяснением")]
		public void SetUserBaseAccess_UnknownLogin_Throws() {
			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess("нет-такого", new DbUserBaseAccess {
					BaseId = baseId, BaseName = BaseName, HasAccess = true
				}));

			Assert.That(exception.Message, Does.Contain("не найден"));
		}

		[Test(Description = "Доступ к несуществующей базе - тоже явный отказ")]
		public void SetUserBaseAccess_UnknownBase_Throws() {
			Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess("worker", new DbUserBaseAccess {
					BaseId = 9999, BaseName = "нет-такой", HasAccess = true
				}));
		}

		[Test(Description = "Обычному пользователю чужие доступы менять нельзя")]
		public void SetUserBaseAccess_ByPlainUser_Throws() {
			AddUser("plain", "plain-pass");
			var plain = LoginAs("plain", "plain-pass");

			Assert.Throws<InvalidOperationException>(
				() => plain.SetUserBaseAccess("worker", new DbUserBaseAccess {
					BaseId = baseId, BaseName = BaseName, HasAccess = true
				}));
		}

		[Test(Description = "Обрыв связи с облаком - исключение с текстом сервера, а не молчание")]
		public void GetUserBaseAccess_CloudUnavailable_ThrowsWithDetail() {
			BreakCloud(StatusCode.Unavailable, "связь потеряна");

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.GetUserBaseAccess("worker"));

			Assert.That(exception.Message, Does.Contain("связь потеряна"));
		}

		[Test(Description = "Облако восстановилось - следующий вызов проходит, переподключаться вручную не нужно")]
		public void Operations_AfterCloudRecovers_WorkAgain() {
			BreakCloud(StatusCode.Unavailable);
			Assert.Throws<InvalidOperationException>(() => provider.GetUserBaseAccess("worker"));

			RepairCloud();

			Assert.DoesNotThrow(() => provider.GetUserBaseAccess("worker"),
				"канал gRPC переживает временную недоступность сам");
		}

		[Test(Description = "Параллельные вызовы через один провайдер друг другу не мешают")]
		[Category("Concurrency")]
		public async Task ParallelCalls_OnSingleProvider_DoNotInterfere() {
			// в отличие от свободного подключения тут нет общего MySqlConnection:
			// канал gRPC потокобезопасен по построению, тест это фиксирует
			Grant("worker", baseId);

			var errors = new ConcurrentBag<Exception>();
			var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() => {
				try {
					if(i % 2 == 0)
						provider.GetUserDatabases();
					else
						provider.GetUserBaseAccess("worker");
				}
				catch(Exception ex) { errors.Add(ex); }
			})).ToList();
			await Task.WhenAll(tasks);

			Assert.That(errors, Is.Empty,
				"первая ошибка: " + (errors.FirstOrDefault()?.Message ?? "нет"));
		}

		[Test(Description = "Сотня баз и сотня пользователей читаются за один вызов каждый")]
		[Category("Performance")]
		public void LargeAccount_ReadsStayFast() {
			for(int i = 0; i < 100; i++)
				AddBase($"bulk_base_{i:D3}");
			for(int i = 0; i < 100; i++)
				AddUser($"bulk_user_{i:D3}");

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			var databases = provider.GetUserDatabases();
			var users = provider.GetUsers();
			stopwatch.Stop();

			Assert.That(databases.Count, Is.GreaterThanOrEqualTo(100));
			Assert.That(users.Count, Is.GreaterThanOrEqualTo(100));
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000),
				$"два вызова к облаку заняли {stopwatch.ElapsedMilliseconds} мс");
		}
	}
}
