using NSubstitute;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Страницы лаунчера поверх облачного подключения - тот же путь «от нажатия кнопки»
	/// и та же сборка страниц <see cref="LauncherPagesHarness"/>, что и у свободного подключения,
	/// только бэкенд за провайдером другой.
	/// </summary>
	[TestFixture(TestOf = typeof(DataBasesVM))]
	public class CloudPages_IntegrationTest : CloudProviderTestFixtureBase {
		private LauncherPagesHarness Pages { get; set; }

		[OneTimeSetUp]
		public void SetUpSchedulers() => LauncherPagesHarness.UseImmediateSchedulers();

		[SetUp]
		public void SetUpPages() =>
			Pages = new LauncherPagesHarness(new QsCloudConnectionTypeBase(), "Облако", TestProductCode);

		[Test(Description = "После входа страница показывает базы из облака")]
		public async Task SetProvider_ShowsCloudDatabases() {
			var db = AddBase("page_base", "Первая");
			Grant(AdminLogin, db.Id);

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.That(page.Databases.Select(d => d.BaseName), Does.Contain("page_base"));
			Assert.That(page.SelectedDatabase, Is.Not.Null, "первая база выбирается сама");
		}

		[Test(Description = "Администратору аккаунта доступны все кнопки, кроме синхронизации")]
		public async Task Capabilities_ForAccountAdmin() {
			AddBase("caps_base");

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.That(page.Capabilities.CanCreate, Is.True);
			Assert.That(page.Capabilities.CanDrop, Is.True);
			Assert.That(page.Capabilities.CanManageUsers, Is.True);
			Assert.That(page.Capabilities.CanRefreshMetadata, Is.False, "реестр ведёт облако - синхронизировать нечего");
		}

		[Test(Description = "Обычному пользователю управление недоступно")]
		public async Task Capabilities_ForPlainUser() {
			AddUser("plain", "plain-pass");

			var page = await Pages.OpenDatabasesPage(LoginAs("plain", "plain-pass"));

			Assert.That(page.Capabilities.CanCreate, Is.False);
			Assert.That(page.Capabilities.CanDrop, Is.False);
			Assert.That(page.Capabilities.CanManageUsers, Is.False);
		}

		[Test(Description = "Кнопка «Удалить базу» с подтверждением убирает её из облака")]
		public async Task DeleteDatabaseCommand_Confirmed_DropsFromCloud() {
			var target = AddBase("to_delete", "На удаление");
			var keep = AddBase("to_keep", "Остаётся");
			Grant(AdminLogin, target.Id);
			Grant(AdminLogin, keep.Id);
			Pages.AnswerYesToQuestions(); // отвечаем «да»

			var page = await Pages.OpenDatabasesPage(LoginAs());
			var selected = page.Databases.First(d => d.BaseName == "to_delete");

			await ((ReactiveCommand<DbInfo, Unit>)page.DeleteDatabaseCommand).Execute(selected);

			Assert.That(State.FindBase(target.Id), Is.Null, "база должна уйти из реестра облака");
			Assert.That(page.Databases.Select(d => d.BaseName), Does.Not.Contain("to_delete"));
			Assert.That(page.Databases.Select(d => d.BaseName), Does.Contain("to_keep"));
			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Success,
				Arg.Is<string>(m => m.Contains("удалена")), Arg.Any<string>());
		}

		[Test(Description = "Отказ в подтверждении оставляет базу на месте")]
		public async Task DeleteDatabaseCommand_Declined_KeepsBase() {
			var target = AddBase("survivor");
			Grant(AdminLogin, target.Id);
			Pages.AnswerNoToQuestions(); // отвечаем «нет»

			var page = await Pages.OpenDatabasesPage(LoginAs());
			var selected = page.Databases.First(d => d.BaseName == "survivor");

			await ((ReactiveCommand<DbInfo, Unit>)page.DeleteDatabaseCommand).Execute(selected);

			Assert.That(State.FindBase(target.Id), Is.Not.Null);
		}

		[Test(Description = "Кнопка «Подключиться» отдаёт запускателю строку и сессию облака")]
		public async Task ConnectCommand_PassesCloudSessionToAppRunner() {
			var db = AddBase("to_run", "Рабочая");
			Grant(AdminLogin, db.Id);

			var page = await Pages.OpenDatabasesPage(LoginAs());
			page.SelectedDatabase = page.Databases.First(d => d.BaseName == "to_run");

			await page.ConnectAsync();

			Pages.AppRunner.Received(1).Run(Arg.Is<LoginToDatabaseResponse>(
				r => r.Success && r.Parameters["SessionId"] == $"session-{db.Id}"));
		}

		[Test(Description = "Кнопка «Пользователи» открывает страницу со списком из облака")]
		public async Task OpenUserManagementCommand_ShowsCloudUsers() {
			AddUser("worker", name: "Работник");
			var page = await Pages.OpenDatabasesPage(LoginAs());

			await page.OpenUserManagementCommand.Execute();

			Assert.That(Pages.PushedPages, Has.Count.EqualTo(1));
			var usersPage = (UsersVM)Pages.PushedPages[0];
			Assert.That(usersPage.CanManageUsers, Is.True);
			Assert.That(usersPage.Users.Select(u => u.Login), Does.Contain("worker"));
		}

		[Test(Description = "Полный путь создания пользователя через форму заводит его в облаке")]
		public async Task CreateUser_ThroughForm_CreatesInCloud() {
			var usersPage = await Pages.OpenUsersPage(LoginAs());

			await usersPage.NewUserCommand.Execute();
			var form = Pages.LastForm;

			form.Card.Login = "created_by_form";
			form.Card.Name = "Через форму";
			form.Card.NewPassword = "form-pass-1234";

			await form.SaveCommand.Execute();

			Assert.That(State.FindUser("created_by_form"), Is.Not.Null, "пользователь должен появиться в облаке");
			Assert.That(State.FindUser("created_by_form").Info.Name, Is.EqualTo("Через форму"));
			Assert.That(Pages.PopCount, Is.GreaterThan(0), "после сохранения форма закрывается");
			Assert.That(usersPage.Users.Select(u => u.Login), Does.Contain("created_by_form"));
		}

		[Test(Description = "Выдача доступа галочкой доходит до реестра облака")]
		public async Task GrantAccess_ThroughForm_ReachesCloud() {
			int baseId = AddBase("form_base", "Рабочая").Id;
			AddUser("form_worker", name: "Работник");

			var usersPage = await Pages.OpenUsersPage(LoginAs());
			usersPage.SelectedUser = usersPage.Users.First(u => u.Login == "form_worker");
			await usersPage.EditUserCommand.Execute();
			var form = await Pages.LastFormLoaded();

			form.BaseAccesses.First(r => r.BaseId == baseId).HasAccess = true; // та самая галочка
			await form.SaveCommand.Execute();

			Assert.That(State.FindAccess("form_worker", baseId)?.HasAccess, Is.True);
		}

		[Test(Description = "Ошибка облака при сохранении показывается пользователю, форма не закрывается")]
		public async Task SaveUser_WhenLoginTaken_ShowsErrorAndKeepsForm() {
			AddUser("occupied"); // логин уже занят

			var usersPage = await Pages.OpenUsersPage(LoginAs());

			await usersPage.NewUserCommand.Execute();
			var form = Pages.LastForm;
			form.Card.Login = "occupied";
			form.Card.NewPassword = "pass-1234";

			await form.SaveCommand.Execute();

			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Error, Arg.Any<string>(), Arg.Any<string>());
			Assert.That(Pages.PopCount, Is.Zero, "форма с ошибкой закрываться не должна");
		}

		[Test(Description = "Недоступное облако страница показывает сообщением, а не падением")]
		public void UsersPage_CloudUnavailable_ShowsError() {
			var provider = LoginAs();
			BreakCloud(Grpc.Core.StatusCode.Unavailable, "облако недоступно");

			Assert.DoesNotThrowAsync(async () => await Pages.OpenUsersPage(provider));

			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Error,
				Arg.Any<string>(), "Управление пользователями");
		}
	}
}
