using Grpc.Core;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using System;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Пользователи облака. Мест хранения тут одно - реестр облака, поэтому вместо тройной сверки
	/// проверяем, что лаунчер отправил и как разобрал ответ, включая отказы.
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_UsersTest : CloudProviderTestFixtureBase {

		[Test(Description = "Созданный пользователь появляется в облаке со всем профилем")]
		public void CreateUser_CreatesUserWithFullProfile() {
			bool created = LoginAs().CreateUser(new DbUserInfo {
				Login = "newbie", Name = "Новичок", Email = "newbie@example.com",
				Phone = "+7 000", Post = "Кладовщик", Comment = "принят на испытательный"
			}, "newbie-pass");

			var stored = State.FindUser("newbie");
			Assert.That(created, Is.True);
			Assert.That(stored, Is.Not.Null);
			Assert.That(stored.Info.Name, Is.EqualTo("Новичок"));
			Assert.That(stored.Info.Email, Is.EqualTo("newbie@example.com"));
			// поля, которых у свободного подключения нет вовсе
			Assert.That(stored.Info.Phone, Is.EqualTo("+7 000"));
			Assert.That(stored.Info.Post, Is.EqualTo("Кладовщик"));
			Assert.That(stored.Info.Comment, Is.EqualTo("принят на испытательный"));
		}

		[Test(Description = "Созданный пользователь может войти в облако по заданному паролю")]
		public void CreateUser_NewUserCanLogIn() {
			LoginAs().CreateUser(new DbUserInfo { Login = "loginable" }, "loginable-pass");

			var response = CreateProvider("loginable", "loginable-pass").LoginToServer();

			Assert.That(response.Success, Is.True, response.ErrorMessage);
		}

		[Test(Description = "Пользователь-администратор заводится с флагом администратора аккаунта")]
		public void CreateUser_AdminFlag_StoredInCloud() {
			LoginAs().CreateUser(new DbUserInfo { Login = "chief", IsAdmin = true }, "chief-pass");

			Assert.That(State.FindUser("chief").Info.IsAccountAdmin, Is.True);
		}

		[Test(Description = "Занятый логин - отказ облака превращается в исключение с его текстом")]
		public void CreateUser_LoginAlreadyTaken_ThrowsWithCloudMessage() {
			AddUser("occupied"); // логин уже есть в облаке
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.CreateUser(new DbUserInfo { Login = "occupied" }, "pass-1234"));

			Assert.That(exception.Message, Does.Contain("уже существует"),
				"пользователю показывается объяснение облака, а не общая ошибка");
		}

		[Test(Description = "Обычному пользователю управление пользователями запрещено")]
		public void CreateUser_ByPlainUser_Throws() {
			AddUser("plain", "plain-pass");
			var provider = LoginAs("plain", "plain-pass");

			Assert.Throws<InvalidOperationException>(
				() => provider.CreateUser(new DbUserInfo { Login = "whoever" }, "pass-1234"));
		}

		[Test(Description = "Правка профиля доходит до облака целиком")]
		public void UpdateUser_Profile_StoredInCloud() {
			AddUser("profiled", name: "Было");

			LoginAs().UpdateUser(new DbUserInfo {
				Login = "profiled", Name = "Стало", Email = "stalo@example.com",
				Phone = "+7 111", Post = "Мастер", Comment = "переведён"
			});

			var stored = State.FindUser("profiled").Info;
			Assert.That(stored.Name, Is.EqualTo("Стало"));
			Assert.That(stored.Email, Is.EqualTo("stalo@example.com"));
			Assert.That(stored.Phone, Is.EqualTo("+7 111"));
			Assert.That(stored.Post, Is.EqualTo("Мастер"));
		}

		[Test(Description = "Смена пароля пользователя пускает его по новому и закрывает старый")]
		public void UpdateUser_NewPassword_TakesEffect() {
			AddUser("repass", "old-pass");

			LoginAs().UpdateUser(new DbUserInfo { Login = "repass" }, "new-pass");

			Assert.That(CreateProvider("repass", "new-pass").LoginToServer().Success, Is.True);
			Assert.That(CreateProvider("repass", "old-pass").LoginToServer().Success, Is.False);
		}

		[Test(Description = "Пустой новый пароль пароль не меняет")]
		public void UpdateUser_EmptyPassword_KeepsOldOne() {
			AddUser("keeppass", "old-pass");

			LoginAs().UpdateUser(new DbUserInfo { Login = "keeppass", Name = "Только имя" });

			Assert.That(CreateProvider("keeppass", "old-pass").LoginToServer().Success, Is.True,
				"правка профиля не должна сбрасывать пароль");
		}

		[Test(Description = "Блокировка пользователя закрывает ему вход")]
		public void UpdateUser_Disabled_BlocksLogin() {
			AddUser("blockme", "blockme-pass");

			LoginAs().UpdateUser(new DbUserInfo {
				Login = "blockme", Disabled = true
			});

			Assert.That(State.FindUser("blockme").Info.Disabled, Is.True);
			Assert.That(CreateProvider("blockme", "blockme-pass").LoginToServer().Success, Is.False);
		}

		[Test(Description = "Правка несуществующего пользователя - отказ облака с объяснением")]
		public void UpdateUser_UnknownLogin_Throws() {
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.UpdateUser(new DbUserInfo { Login = "нет-такого" }));

			Assert.That(exception.Message, Does.Contain("не найден"));
		}

		[Test(Description = "Удаление пользователя убирает его из облака вместе с доступами")]
		public void DeleteUser_RemovesUserAndAccess() {
			var db = AddBase("some_base");
			AddUser("condemned");
			Grant("condemned", db.Id);

			bool deleted = LoginAs().DeleteUser("condemned");

			Assert.That(deleted, Is.True);
			Assert.That(State.FindUser("condemned"), Is.Null);
			Assert.That(State.Access.Any(a => a.Login == "condemned"), Is.False,
				"доступы удаляются вместе с пользователем - в облаке это одна операция");
		}

		[Test(Description = "Удаление несуществующего пользователя - объяснение, а не молчание")]
		public void DeleteUser_UnknownLogin_Throws() {
			var provider = LoginAs();

			Assert.Throws<InvalidOperationException>(() => provider.DeleteUser("нет-такого"));
		}

		[Test(Description = "Список пользователей приходит из облака с флагами")]
		public void GetUsers_ReturnsCloudUsers() {
			AddUser("worker", name: "Работник");
			AddUser("boss", isAdmin: true, name: "Начальник");

			var users = LoginAs().GetUsers();

			Assert.That(users.Select(u => u.Login), Is.SupersetOf(new[] { "worker", "boss" }));
			Assert.That(users.First(u => u.Login == "boss").IsAdmin, Is.True);
			Assert.That(users.First(u => u.Login == "worker").Name, Is.EqualTo("Работник"));
		}

		[Test(Description = "Собственная учётка помечена - её нельзя удалить из интерфейса")]
		public void GetUsers_MarksCurrentUser() {
			AddUser("other");

			var users = LoginAs().GetUsers();

			Assert.That(users.First(u => u.Login == AdminLogin).IsCurrentUser, Is.True);
			Assert.That(users.First(u => u.Login == "other").IsCurrentUser, Is.False);
		}

		[Test(Description = "Смена собственного пароля работает у обычного пользователя")]
		public void ChangeOwnPassword_PlainUser_Works() {
			AddUser("selfchanger", "first-pass");
			var self = LoginAs("selfchanger", "first-pass");

			bool changed = self.ChangeOwnPassword("second-pass");

			Assert.That(changed, Is.True);
			Assert.That(CreateProvider("selfchanger", "second-pass").LoginToServer().Success, Is.True);
			Assert.That(CreateProvider("selfchanger", "first-pass").LoginToServer().Success, Is.False);
		}

		[Test(Description = "Сбой облака при чтении списка - исключение с текстом сервера")]
		public void GetUsers_CloudFailure_ThrowsWithServerDetail() {
			var provider = LoginAs();
			BreakCloud(StatusCode.Unavailable, "служба пользователей недоступна");

			var exception = Assert.Throws<InvalidOperationException>(() => provider.GetUsers());

			Assert.That(exception.Message, Does.Contain("служба пользователей недоступна"));
		}
	}
}
