using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;

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

		[Test(Description = "Облако недоступно - отдельное сообщение")]
		public void LoginToServer_CloudUnavailable_ReturnsFailureResponse() {
			LoginClient.Start(Arg.Any<string>()).Throws(Refusal(StatusCode.Unavailable, "связи нет"));

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Не удалось подключиться к облаку"));
		}
	}
}
