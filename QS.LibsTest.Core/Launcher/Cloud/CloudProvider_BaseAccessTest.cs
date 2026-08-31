using NSubstitute;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	/// <summary>Доступ к базам</summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_BaseAccessTest : CloudProviderTestFixtureBase {
		private const string Worker = "worker";
		private const int BaseId = 7;

		[Test(Description = "Список доступов приходит с флагами по каждой базе")]
		public void GetUserBaseAccess_MapsFlagsPerBase() {
			UserClient.GetUserBaseAccess(Worker, TestProductCode).Returns(new List<BaseAccessInfo> {
				new BaseAccessInfo { BaseId = BaseId, BaseTitle = "Рабочая", HasAccess = true, Admin = true, ReadOnly = true },
				new BaseAccessInfo { BaseId = 8, BaseTitle = "Вторая", HasAccess = false }
			});

			var rows = LoginAs().GetUserBaseAccess(Worker);

			Assert.That(rows.Select(r => r.BaseId), Is.EquivalentTo(new[] { BaseId, 8 }));
			var granted = rows.Single(r => r.BaseId == BaseId);
			Assert.That(granted.Title, Is.EqualTo("Рабочая"));
			Assert.That(granted.HasAccess, Is.True);
			Assert.That(granted.IsAdmin, Is.True, "Admin облака - это IsAdmin в карточке доступа");
			Assert.That(granted.ReadOnly, Is.True);
			Assert.That(rows.Single(r => r.BaseId == 8).HasAccess, Is.False);
		}

		[TestCase(false, false, TestName = "Доступ без дополнительных прав")]
		[TestCase(true, false, TestName = "Администратор базы")]
		[TestCase(false, true, TestName = "Только чтение")]
		public void SetUserBaseAccess_Granted_SendsFlags(bool admin, bool readOnly) {
			UserClient.ChangeBaseAccess(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
				Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<uint>())
				.Returns(new ChangeBaseAccessResponse { Success = true });

			LoginAs().SetUserBaseAccess(Worker, new DbUserBaseAccess {
				BaseId = BaseId, HasAccess = true, IsAdmin = admin, ReadOnly = readOnly
			});

			UserClient.Received(1).ChangeBaseAccess(Worker, BaseId, true, admin, readOnly, TestProductCode);
		}

		[Test(Description = "Снятие доступа уходит тем же вызовом с grant = false")]
		public void SetUserBaseAccess_Revoked_SendsGrantFalse() {
			UserClient.ChangeBaseAccess(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
				Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<uint>())
				.Returns(new ChangeBaseAccessResponse { Success = true });

			LoginAs().SetUserBaseAccess(Worker, new DbUserBaseAccess { BaseId = BaseId, HasAccess = false });

			UserClient.Received(1).ChangeBaseAccess(Worker, BaseId, false, false, false, TestProductCode);
		}

		[Test(Description = "Отказ облака объясняется его же текстом")]
		public void SetUserBaseAccess_Refused_ThrowsWithCloudMessage() {
			UserClient.ChangeBaseAccess(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
				Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<uint>())
				.Returns(new ChangeBaseAccessResponse { Success = false, Message = "База не найдена" });
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess(Worker, new DbUserBaseAccess { BaseId = 9999, HasAccess = true }));

			Assert.That(exception.Message, Is.EqualTo("База не найдена"));
		}

		[Test(Description = "Отказ без текста всё равно объясняется - пустое сообщение пользователю бесполезно")]
		public void SetUserBaseAccess_RefusedWithoutMessage_ThrowsWithFallbackText() {
			UserClient.ChangeBaseAccess(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
				Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<uint>())
				.Returns(new ChangeBaseAccessResponse { Success = false, Message = string.Empty });
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.SetUserBaseAccess(Worker, new DbUserBaseAccess { BaseId = BaseId, HasAccess = true }));

			Assert.That(exception.Message, Does.Contain("Не удалось изменить доступ"));
		}
	}
}
