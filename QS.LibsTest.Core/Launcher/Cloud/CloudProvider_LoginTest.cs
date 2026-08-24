using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using System;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Вход в облако. Отказ здесь ожидаемый, поэтому провайдер обязан вернуть Response
	/// с текстом для пользователя, а не бросить исключение
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_LoginTest : CloudProviderTestFixtureBase {

		[Test(Description = "Администратор аккаунта получает права управления")]
		public void LoginToServer_Admin_GetsAdminRights() {
			var provider = LoginAs(isAdmin: true);

			Assert.That(provider.IsAdmin, Is.True);
			Assert.That(provider.CanManageUsers, Is.True);
			Assert.That(provider.CanCreateDatabase, Is.True);
		}

		[Test(Description = "Обычному пользователю управление недоступно")]
		public void LoginToServer_PlainUser_HasNoManagementRights() {
			var provider = LoginAs("plain", isAdmin: false);

			Assert.That(provider.IsAdmin, Is.False);
			Assert.That(provider.CanManageUsers, Is.False);
			Assert.That(provider.CanCreateDatabase, Is.False);
		}

		// Неверный пароль и неизвестный логин облако различает само, а для лаунчера это один
		// и тот же отказ - проверяем оба кода, которыми оно об этом сообщает
		[TestCase(StatusCode.Unauthenticated, TestName = "Неверный логин или пароль")]
		[TestCase(StatusCode.PermissionDenied, TestName = "Учётная запись отключена")]
		public void LoginToServer_Refused_ReturnsFailureResponse(StatusCode code) {
			LoginClient.Start(Arg.Any<string>()).Throws(Refusal(code, "нет доступа"));

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Неверные данные для входа"));
			Assert.That(response.ErrorMessage, Does.Contain("нет доступа"), "текст сервера должен дойти до пользователя");
		}

		[Test(Description = "Облако недоступно - отдельное сообщение, не про неверный пароль")]
		public void LoginToServer_CloudUnavailable_ReturnsFailureResponse() {
			LoginClient.Start(Arg.Any<string>()).Throws(Refusal(StatusCode.Unavailable, "связи нет"));

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Не удалось подключиться к облаку"));
		}

		[Test(Description = "Облако просит обновить лаунчер - вход проходит, флаг доезжает")]
		public void LoginToServer_OutdatedLauncher_ReportsNeedUpdate() {
			LoginClient.Start(Arg.Any<string>())
				.Returns(new QS.Cloud.Core.StartResponse { YouAccountAdmin = true, NeedUpdateLauncher = true });

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.True);
			Assert.That(response.NeedToUpdateLauncher, Is.True);
		}

		[Test(Description = "Синхронизация метаинформации облаку не нужна и вызовом не поддерживается")]
		public void RefreshMetadata_IsNotOfferedAndRefusesCall() {
			var provider = LoginAs();

			Assert.That(provider.CanRefreshMetadata, Is.False);
			Assert.Throws<InvalidOperationException>(() => provider.RefreshMetadata());
		}

		[Test(Description = "Конструктор в облако не ходит - соединение открывает только вход")]
		public void CreateProvider_ConstructorDoesNotConnect() {
			var provider = CreateProvider();

			LoginClient.DidNotReceive().Start(Arg.Any<string>());
			Assert.That(provider.Account, Is.EqualTo(AccountName));
		}

		[Test(Description = "Облако ведёт полный профиль, поэтому форма показывает все поля")]
		public void SupportedUserFields_CoversFullProfile() {
			var fields = LoginAs().SupportedUserFields;

			Assert.That(fields.HasFlag(DbUserFields.Phone), Is.True);
			Assert.That(fields.HasFlag(DbUserFields.Post), Is.True);
			Assert.That(fields.HasFlag(DbUserFields.Comment), Is.True);
			Assert.That(fields.HasFlag(DbUserFields.AdminFlag), Is.True);
		}
	}
}
