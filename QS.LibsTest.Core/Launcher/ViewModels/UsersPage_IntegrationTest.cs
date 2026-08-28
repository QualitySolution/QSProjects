using NSubstitute;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Launcher.ViewModels.PageViewModels;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.ViewModels {
	[TestFixture(TestOf = typeof(UsersVM))]
	public class UsersPage_IntegrationTest : LauncherViewModelTestFixtureBase {
		private const string BaseName = "base_users_page";

		private MariaDBProvider provider;
		private int baseId;

		[SetUp]
		public async Task SetUpScenario() {
			await CreateApplicationDatabase(BaseName, "Рабочая база");
			baseId = await SeedMetabase(BaseName, "Рабочая база");

			provider = LoginAs();
		}

		/// <summary>Открывает форму нового пользователя так же, как это делает кнопка Создать</summary>
		private async Task<UserManagementVM> OpenNewUserForm(UsersVM usersPage) {
			await usersPage.NewUserCommand.Execute();
			return Pages.LastForm;
		}

		[Test(Description = "Открытие страницы показывает список пользователей")]
		public async Task SetProvider_ShowsUserList() {
			await SeedMetabaseUser("existing_one", name: "Первый");
			await SeedMetabaseUser("existing_two", name: "Второй");

			var page = await Pages.OpenUsersPage(provider);

			Assert.That(page.CanManageUsers, Is.True);
			Assert.That(page.Users.Select(u => u.Login),
				Is.SupersetOf(new[] { "existing_one", "existing_two" }));
		}

		[Test(Description = "Полный путь создания")]
		public async Task CreateUser_ThroughForm_CreatesEverywhere() {
			var page = await Pages.OpenUsersPage(provider);
			var form = await OpenNewUserForm(page);

			// заполняем форму так же, как это делает пользователь
			form.Card.Login = "created_by_form";
			form.Card.Name = "Через форму";
			form.Card.Email = "form@example.com";
			form.Card.NewPassword = "form-pass-1234";

			await form.SaveCommand.Execute();

			bool onServer = await ServerLoginExists("created_by_form");
			var metabaseRow = await ReadMetabaseUser("created_by_form");

			Assert.That(onServer, Is.True, "учётка должна появиться на сервере");
			Assert.That(metabaseRow, Is.Not.Null, "и в метабазе");
			Assert.That(metabaseRow?.Name, Is.EqualTo("Через форму"));
			Assert.That(Pages.PopCount, Is.GreaterThan(0), "после сохранения форма должна закрыться");
			Assert.That(page.Users.Select(u => u.Login), Does.Contain("created_by_form"),
				"список на странице должен обновиться сам");
		}

		[Test(Description = "Без пароля кнопка Сохранить нового пользователя недоступна")]
		public async Task CreateUser_WithoutPassword_SaveDisabled() {
			var page = await Pages.OpenUsersPage(provider);
			var form = await OpenNewUserForm(page);

			form.Card.Login = "no_password"; // пароль не задан

			bool canSave = await form.SaveCommand.CanExecute.FirstAsync();
			Assert.That(canSave, Is.False, "новому пользователю пароль обязателен");
		}

		[Test(Description = "Без логина кнопка Сохранить недоступна")]
		public async Task CreateUser_WithoutLogin_SaveDisabled() {
			var page = await Pages.OpenUsersPage(provider);
			var form = await OpenNewUserForm(page);

			form.Card.NewPassword = "pass-1234"; // логин не задан

			bool canSave = await form.SaveCommand.CanExecute.FirstAsync();
			Assert.That(canSave, Is.False);
		}

		[Test(Description = "Выдача доступа к базе галочкой доходит до сервера, метабазы и таблицы users")]
		public async Task GrantAccess_ThroughForm_ReachesAllThreePlaces() {
			provider.CreateUser(new DbUserInfo { Login = "form_worker", Name = "Работник" }, "worker-pass");

			var page = await Pages.OpenUsersPage(provider);
			page.SelectedUser = page.Users.First(u => u.Login == "form_worker");
			await page.EditUserCommand.Execute();
			var form = await Pages.LastFormLoaded();

			var row = form.BaseAccesses.First(r => r.BaseName == BaseName);
			Assert.That(row.HasAccess, Is.False, "предусловие: доступа ещё нет");
			row.HasAccess = true;		//галочка

			await form.SaveCommand.Execute();

			var grants = await ReadServerGrants("form_worker");
			var rights = await ReadBaseUpdateRights();
			var baseUser = await ReadBaseUser(BaseName, "form_worker");

			Assert.That(GrantsMentionDatabase(grants, BaseName), Is.True,
				"грант на сервере");
			Assert.That(rights.Any(r => r.Login == "form_worker" && r.BaseName == BaseName), Is.True,
				"строка в base_update_rights");
			Assert.That(baseUser?.Deactivated, Is.False, "живая строка в users базы");
		}

		[Test(Description = "Галочка только чтение ограничивает выданные привилегии")]
		public async Task GrantReadOnlyAccess_ThroughForm_GrantsSelectOnly() {
			provider.CreateUser(new DbUserInfo { Login = "form_reader", Name = "Читатель" }, "reader-pass");

			var page = await Pages.OpenUsersPage(provider);
			page.SelectedUser = page.Users.First(u => u.Login == "form_reader");
			await page.EditUserCommand.Execute();
			var form = await Pages.LastFormLoaded();

			form.BaseAccesses.First(r => r.BaseName == BaseName).ReadOnly = true; // галочка
			await form.SaveCommand.Execute();

			var grants = await ReadServerGrants("form_reader");
			string baseGrant = FindGrantOnDatabase(grants, BaseName);

			Assert.That(baseGrant, Does.Contain("SELECT"));
			Assert.That(baseGrant, Does.Not.Contain("INSERT"));
		}

		[Test(Description = "Снятие галочки доступа отзывает права и деактивирует пользователя в базе")]
		public async Task RevokeAccess_ThroughForm_DeactivatesUserInBase() {
			provider.CreateUser(new DbUserInfo { Login = "form_leaver", Name = "Уходящий" }, "leaver-pass");
			provider.SetUserBaseAccess("form_leaver", new DbUserBaseAccess {
				BaseName = BaseName, BaseId = baseId, HasAccess = true, Name = "Уходящий"
			});

			var page = await Pages.OpenUsersPage(provider);
			page.SelectedUser = page.Users.First(u => u.Login == "form_leaver");
			await page.EditUserCommand.Execute();
			var form = await Pages.LastFormLoaded();

			form.BaseAccesses.First(r => r.BaseName == BaseName).HasAccess = false; // снимаем галочку доступа
			await form.SaveCommand.Execute();

			var baseUser = await ReadBaseUser(BaseName, "form_leaver");
			var rights = await ReadBaseUpdateRights();

			Assert.That(baseUser?.Deactivated, Is.True, "в базе пользователь должен стать отключённым");
			Assert.That(rights.Any(r => r.Login == "form_leaver" && r.BaseName == BaseName), Is.False);
		}

		[Test(Description = "Кнопка Удалить с подтверждением убирает пользователя и обновляет список")]
		public async Task DeleteUserCommand_Confirmed_RemovesUserAndRefreshes() {
			provider.CreateUser(new DbUserInfo { Login = "doomed" }, "doomed-pass");
			Pages.AnswerYesToQuestions();

			var page = await Pages.OpenUsersPage(provider);
			page.SelectedUser = page.Users.First(u => u.Login == "doomed");

			await page.DeleteUserCommand.Execute();

			bool onServer = await ServerLoginExists("doomed");
			Assert.That(onServer, Is.False);
			Assert.That(page.Users.Select(u => u.Login), Does.Not.Contain("doomed"));
			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Success,
				Arg.Any<string>(), "Управление пользователями");
		}

		[Test(Description = "Отказ в подтверждении оставляет пользователя на месте")]
		public async Task DeleteUserCommand_Declined_KeepsUser() {
			provider.CreateUser(new DbUserInfo { Login = "spared" }, "spared-pass");
			Pages.AnswerNoToQuestions();

			var page = await Pages.OpenUsersPage(provider);
			page.SelectedUser = page.Users.First(u => u.Login == "spared");

			await page.DeleteUserCommand.Execute();

			Assert.That(await ServerLoginExists("spared"), Is.True);
		}
	}
}
