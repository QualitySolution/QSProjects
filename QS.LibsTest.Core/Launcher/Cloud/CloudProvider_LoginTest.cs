using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;

namespace QS.Launcher.Test.Cloud {
	/// <summary>Вход в облако</summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_LoginTest : CloudProviderTestFixtureBase {

		// единственный флаг, которым облако управляет правами: из него выводятся и управление
		// пользователями, и создание баз. Обычный пользователь не должен увидеть ни того, ни другого
		[TestCase(true, TestName = "Администратор аккаунта получает права управления")]
		[TestCase(false, TestName = "Обычному пользователю управление недоступно")]
		public void LoginToServer_AdminFlagFromCloud_DrivesRights(bool isAdmin) {
			var provider = LoginAs("someone", isAdmin);

			Assert.That(provider.IsAdmin, Is.EqualTo(isAdmin));
			Assert.That(provider.CanManageUsers, Is.EqualTo(isAdmin));
			Assert.That(provider.CanCreateDatabase, Is.EqualTo(isAdmin));
		}

		[TestCase(StatusCode.Unauthenticated, TestName = "Неверный логин или пароль")]
		[TestCase(StatusCode.PermissionDenied, TestName = "Учётная запись отключена")]
		public void LoginToServer_Refused_ReturnsFailureResponse(StatusCode code) {
			LoginClient.Start(Arg.Any<string>()).Throws(Refusal(code, "нет доступа"));

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Неверные данные для входа"));
			Assert.That(response.ErrorMessage, Does.Contain("нет доступа"), "текст сервера должен дойти до пользователя");
		}

		[Test(Description = "Смена пароля переподписывает все клиенты, а не только тот, через который меняли")]
		public void ChangeOwnPassword_Succeeded_UpdatesEveryClient() {
			LoginClient.ChangePassword("n3w").Returns(new ChangePasswordResponse { Success = true });
			var provider = LoginAs();

			Assert.That(provider.ChangeOwnPassword("n3w"), Is.True);

			// пропущенный клиент отвалится с Unauthenticated на первом же запросе
			LoginClient.Received(1).UpdatePassword("n3w");
			DbClient.Received(1).UpdatePassword("n3w");
			UserClient.Received(1).UpdatePassword("n3w");
		}

		[Test(Description = "Облако пароль не сменило - заголовки остаются прежними")]
		public void ChangeOwnPassword_Refused_KeepsOldHeaders() {
			LoginClient.ChangePassword(Arg.Any<string>()).Returns(new ChangePasswordResponse { Success = false });
			var provider = LoginAs();

			Assert.That(provider.ChangeOwnPassword("n3w"), Is.False);

			// иначе клиент подписался бы паролем, которого в облаке нет
			LoginClient.DidNotReceive().UpdatePassword(Arg.Any<string>());
			DbClient.DidNotReceive().UpdatePassword(Arg.Any<string>());
			UserClient.DidNotReceive().UpdatePassword(Arg.Any<string>());
		}

		[Test(Description = "Облако недоступно - отдельное сообщение")]
		public void LoginToServer_CloudUnavailable_ReturnsFailureResponse() {
			LoginClient.Start(Arg.Any<string>()).Throws(Refusal(StatusCode.Unavailable, "связи нет"));

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Не удалось подключиться к облаку"));
		}
	}
}
