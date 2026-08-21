using Grpc.Core;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using System;
using System.Diagnostics;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Вход в облако и вычисление прав. В отличие от свободного подключения права приходят
	/// готовым флагом от сервера, а не выводятся из грантов.
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_LoginTest : CloudProviderTestFixtureBase {

		[Test(Description = "Администратор аккаунта входит и получает полный набор прав")]
		public void LoginToServer_Admin_GetsAdminRights() {
			var provider = CreateProvider();

			var response = provider.LoginToServer();

			Assert.That(response.Success, Is.True, response.ErrorMessage);
			Assert.That(provider.IsAdmin, Is.True);
			Assert.That(provider.CanCreateDatabase, Is.True);
			Assert.That(provider.CanDropDatabase, Is.True);
			Assert.That(provider.CanManageUsers, Is.True);
			Assert.That(provider.CanManageBaseAccess, Is.True);
		}

		[Test(Description = "Обычный пользователь входит, но управлять ничем не может")]
		public void LoginToServer_PlainUser_HasNoManagementRights() {
			AddUser("plain", "plain-pass"); // без флага администратора аккаунта

			var provider = CreateProvider("plain", "plain-pass");
			var response = provider.LoginToServer();

			Assert.That(response.Success, Is.True, response.ErrorMessage);
			Assert.That(provider.IsAdmin, Is.False);
			Assert.That(provider.CanCreateDatabase, Is.False, "создание баз только администратору аккаунта");
			Assert.That(provider.CanDropDatabase, Is.False);
			Assert.That(provider.CanManageUsers, Is.False);
		}

		[Test(Description = "Неверный пароль - отказ Response-объектом с понятным текстом")]
		public void LoginToServer_WrongPassword_ReturnsFailureResponse() {
			var provider = CreateProvider(AdminLogin, "не-тот-пароль");

			LoginToServerResponseIsFailure(provider, "Неверные данные для входа");
		}

		[Test(Description = "Неизвестный логин отвергается так же, как неверный пароль")]
		public void LoginToServer_UnknownLogin_ReturnsFailureResponse() {
			var provider = CreateProvider("нет-такого", "любой");

			LoginToServerResponseIsFailure(provider, "Неверные данные для входа");
		}

		[Test(Description = "Отключённая учётная запись в облако не пускает")]
		public void LoginToServer_DisabledUser_Fails() {
			AddUser("disabled_user", "pass-1234", disabled: true);

			var provider = CreateProvider("disabled_user", "pass-1234");
			var response = provider.LoginToServer();

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Неверные данные для входа"),
				"PermissionDenied облако тоже подаёт как отказ во входе");
		}

		[Test(Description = "Недоступное облако - отказ Response-объектом, а не исключением")]
		public void LoginToServer_CloudUnavailable_ReturnsFailureResponse() {
			BreakCloud(StatusCode.Unavailable, "сервис на обслуживании");

			var provider = CreateProvider();
			LoginToServerResponse response = null;
			Assert.DoesNotThrow(() => response = provider.LoginToServer());

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain("Не удалось подключиться к облаку"));
			Assert.That(response.ErrorMessage, Does.Contain("сервис на обслуживании"),
				"пояснение сервера понятнее текста самого gRPC, поэтому показываем его");
		}

		[Test(Description = "Облако может потребовать обновить лаунчер")]
		public void LoginToServer_OutdatedLauncher_ReportsNeedUpdate() {
			State.NeedUpdateLauncher = true;

			var response = CreateProvider().LoginToServer();

			Assert.That(response.Success, Is.True);
			Assert.That(response.NeedToUpdateLauncher, Is.True);
		}

		[Test(Description = "Синхронизация метаинформации в облаке не предлагается, а вызов мимо флага - ошибка")]
		public void RefreshMetadata_IsNotOfferedAndRefusesCall() {
			var provider = LoginAs();

			Assert.That(provider.CanRefreshMetadata, Is.False, "кнопки синхронизации в облаке быть не должно");
			var exception = Assert.Throws<InvalidOperationException>(() => provider.RefreshMetadata(),
				"вызов в обход флага - нарушение контракта, а не тихое «ок»");
			Assert.That(exception.Message, Does.Contain("Облако"), "текст ошибки русский, а не заглушка .NET");
		}

		[Test(Description = "Конструктор провайдера не ходит в сеть: до входа он обязан быть мгновенным")]
		public void CreateProvider_ConstructorDoesNotConnect() {
			BreakCloud(StatusCode.Unavailable); // даже с мёртвым облаком конструктор обязан вернуться сразу

			var stopwatch = Stopwatch.StartNew();
			CreateProvider();
			stopwatch.Stop();

			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
		}

		[Test(Description = "Набор редактируемых полей у облака шире, чем у свободного подключения")]
		public void SupportedUserFields_CoversFullProfile() {
			var provider = LoginAs();

			Assert.That(provider.SupportedUserFields.HasFlag(DbUserFields.Phone), Is.True);
			Assert.That(provider.SupportedUserFields.HasFlag(DbUserFields.Post), Is.True);
			Assert.That(provider.SupportedUserFields.HasFlag(DbUserFields.Comment), Is.True);
			Assert.That(provider.SupportedUserFields.HasFlag(DbUserFields.Disabling), Is.True);
		}

		private static void LoginToServerResponseIsFailure(QSCloudProvider provider, string expectedText) {
			LoginToServerResponse response = null;
			Assert.DoesNotThrow(() => response = provider.LoginToServer(),
				"ожидаемый отказ на границе с облаком должен приходить Response-объектом");

			Assert.That(response.Success, Is.False);
			Assert.That(response.ErrorMessage, Does.Contain(expectedText));
		}
	}
}
