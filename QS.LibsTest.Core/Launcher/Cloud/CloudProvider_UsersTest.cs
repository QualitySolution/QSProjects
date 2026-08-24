using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Управление пользователями: что провайдер отправляет в облако и как разбирает ответ.
	/// Правила облака (кто кому может менять профиль) проверяет само облако - это не наш код
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_UsersTest : CloudProviderTestFixtureBase {

		[Test(Description = "Профиль из облака доезжает до карточки целиком")]
		public void GetUsers_MapsFullProfile() {
			UserClient.GetUsers().Returns(new List<UserInfo> {
				new UserInfo {
					Login = "worker", Name = "Работник", Email = "w@example.com", Phone = "+7-900",
					Post = "Кладовщик", Comment = "примечание", Disabled = true, IsAccountAdmin = true
				}
			});

			var user = LoginAs().GetUsers().Single();

			Assert.That(user.Login, Is.EqualTo("worker"));
			Assert.That(user.Name, Is.EqualTo("Работник"));
			Assert.That(user.Email, Is.EqualTo("w@example.com"));
			Assert.That(user.Phone, Is.EqualTo("+7-900"));
			Assert.That(user.Post, Is.EqualTo("Кладовщик"));
			Assert.That(user.Comment, Is.EqualTo("примечание"));
			Assert.That(user.Disabled, Is.True);
			Assert.That(user.IsAdmin, Is.True, "администратор аккаунта в облаке - это IsAdmin в карточке");
		}

		[Test(Description = "Себя в списке пользователь видит помеченным - его нельзя удалить")]
		public void GetUsers_MarksCurrentUser() {
			UserClient.GetUsers().Returns(new List<UserInfo> { User(AdminLogin), User("someone") });

			var users = LoginAs().GetUsers();

			Assert.That(users.Single(u => u.Login == AdminLogin).IsCurrentUser, Is.True);
			Assert.That(users.Single(u => u.Login == "someone").IsCurrentUser, Is.False);
		}

		[Test(Description = "Создание отправляет в облако весь профиль и пароль")]
		public void CreateUser_SendsProfileAndPassword() {
			UserClient.CreateUser(Arg.Any<UserInfo>(), Arg.Any<string>())
				.Returns(new CreateUserResponse { Success = true });

			LoginAs().CreateUser(new DbUserInfo {
				Login = "newbie", Name = "Новичок", Email = "n@example.com", IsAdmin = true
			}, "newbie-pass");

			UserClient.Received(1).CreateUser(
				Arg.Is<UserInfo>(u => u.Login == "newbie" && u.Name == "Новичок"
					&& u.Email == "n@example.com" && u.IsAccountAdmin),
				"newbie-pass");
		}

		[Test(Description = "Незаполненные поля уходят пустыми строками - protobuf не принимает null")]
		public void CreateUser_EmptyProfile_SendsEmptyStringsNotNull() {
			UserClient.CreateUser(Arg.Any<UserInfo>(), Arg.Any<string>())
				.Returns(new CreateUserResponse { Success = true });

			LoginAs().CreateUser(new DbUserInfo { Login = "bare" }, null);

			UserClient.Received(1).CreateUser(
				Arg.Is<UserInfo>(u => u.Name == "" && u.Email == "" && u.Phone == ""
					&& u.Post == "" && u.Comment == ""),
				Arg.Any<string>());
		}

		[Test(Description = "Отказ облака при создании становится исключением с его же текстом")]
		public void CreateUser_CloudRefused_ThrowsWithCloudMessage() {
			UserClient.CreateUser(Arg.Any<UserInfo>(), Arg.Any<string>())
				.Returns(new CreateUserResponse { Success = false, Message = "Логин уже занят" });
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.CreateUser(new DbUserInfo { Login = "occupied" }, "pass"));

			Assert.That(exception.Message, Is.EqualTo("Логин уже занят"));
		}

		[Test(Description = "Правка отправляет профиль и новый пароль")]
		public void UpdateUser_SendsProfileAndNewPassword() {
			UserClient.UpdateUser(Arg.Any<UserInfo>(), Arg.Any<string>())
				.Returns(new UpdateUserResponse { Success = true });

			LoginAs().UpdateUser(new DbUserInfo { Login = "worker", Name = "Новое имя", Disabled = true }, "new-pass");

			UserClient.Received(1).UpdateUser(
				Arg.Is<UserInfo>(u => u.Login == "worker" && u.Name == "Новое имя" && u.Disabled),
				"new-pass");
		}

		[Test(Description = "Удаление отправляет логин и разбирает отказ")]
		public void DeleteUser_UnknownLogin_ThrowsWithCloudMessage() {
			UserClient.DeleteUser("нет-такого")
				.Returns(new QS.Cloud.Core.DeleteUserResponse { Success = false, Message = "Пользователь не найден" });
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(() => provider.DeleteUser("нет-такого"));

			Assert.That(exception.Message, Is.EqualTo("Пользователь не найден"));
		}

		[Test(Description = "Смена своего пароля уходит в облако")]
		public void ChangeOwnPassword_SendsNewPassword() {
			LoginClient.ChangePassword("second-pass")
				.Returns(new QS.Cloud.Core.ChangePasswordResponse { Success = true });

			Assert.That(LoginAs().ChangeOwnPassword("second-pass"), Is.True);
			LoginClient.Received(1).ChangePassword("second-pass");
		}

		[Test(Description = "Обрыв связи превращается в исключение с текстом сервера, а не в молчание")]
		public void GetUsers_CloudUnavailable_ThrowsWithServerDetail() {
			UserClient.GetUsers().Throws(Refusal(StatusCode.Unavailable, "связь потеряна"));
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(() => provider.GetUsers());

			Assert.That(exception.Message, Does.Contain("связь потеряна"));
		}
	}
}
