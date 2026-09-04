using NSubstitute;
using NUnit.Framework;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.ViewModels {
	/// <summary>от нажатия кнопки до состояния сервера и метабазы</summary>
	[TestFixture(TestOf = typeof(DataBasesVM))]
	public class DataBasesPage_IntegrationTest : LauncherViewModelTestFixtureBase {

		/// <summary>База на сервере и её запись в каталоге метабазы</summary>
		private async Task<int> SeedBaseWithAccess(string baseName, string title) {
			await CreateApplicationDatabase(baseName, title);
			return await SeedMetabase(baseName, title);
		}

		[Test(Description = "После входа страница показывает доступные базы и выбирает первую")]
		public async Task SetProvider_AfterLogin_ShowsAvailableDatabases() {
			await SeedBaseWithAccess("base_page_one", "Первая");

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.That(page.Databases.Select(d => d.BaseName), Does.Contain("base_page_one"));
			Assert.That(page.SelectedDatabase, Is.Not.Null, "первая база должна выбраться сама");
		}

		[Test(Description = "Кнопки страницы включаются по правам администратора")]
		public async Task Capabilities_ForAdmin_AllButtonsEnabled() {
			await SeedBaseWithAccess("base_caps", "Права");

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.That(page.Capabilities.CanCreate, Is.True);
			Assert.That(page.Capabilities.CanDrop, Is.True);
			Assert.That(page.Capabilities.CanRefreshMetadata, Is.True);
			Assert.That(page.Capabilities.CanManageUsers, Is.True);
			Assert.That(page.Capabilities.CanManageDatabases, Is.True);
		}

		[Test(Description = "Без скрипта создания гаснет только создание - импорт дампа скрипта не требует")]
		public async Task Capabilities_WithoutCreationScript_HidesCreateOnly() {
			Pages.ScriptsConfiguration.HasCreationScript().Returns(false); // приложение скрипт не принесло

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.Multiple(() => {
				Assert.That(page.Capabilities.CanCreate, Is.False, "создавать базу нечем - наполнять её будет некому");
				Assert.That(page.Capabilities.CanImport, Is.True);
			});
		}

		[Test(Description = "Без зарегистрированной модели наполнения гаснут и создание, и импорт")]
		public async Task Capabilities_WithoutCreationModel_HidesCreateAndImport() {
			Pages.CreationMap = null; // приложение не зарегистрировало ни одной модели

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.Multiple(() => {
				Assert.That(page.Capabilities.CanCreate, Is.False);
				Assert.That(page.Capabilities.CanImport, Is.False, "принять дамп тоже некому");
			});
		}

		[Test(Description = "Пользователю без прав кнопки управления недоступны")]
		public async Task Capabilities_ForLimitedUser_ManagementDisabled() {
			await CreateServerLogin("viewer", "viewer-pass");
			await SeedMetabaseUser("viewer");
			await SeedBaseWithAccess("base_for_viewer", "Только смотреть");

			var page = await Pages.OpenDatabasesPage(LoginAs("viewer", "viewer-pass"));

			Assert.That(page.Capabilities.CanCreate, Is.False);
			Assert.That(page.Capabilities.CanDrop, Is.False);
			Assert.That(page.Capabilities.CanManageUsers, Is.False);
			Assert.That(page.Capabilities.CanRefreshMetadata, Is.False);
		}

		[Test(Description = "Кнопка Удалить базу")]
		public async Task DeleteDatabaseCommand_Confirmed_DropsDatabaseAndRefreshesList() {
			await SeedBaseWithAccess("base_to_delete", "На удаление");
			await SeedBaseWithAccess("base_to_keep", "Остаётся"); // соседняя база не должна пострадать
			Pages.AnswerYesToQuestions(); // на диалог подтверждения отвечаем да

			var page = await Pages.OpenDatabasesPage(LoginAs());
			var target = page.Databases.First(d => d.BaseName == "base_to_delete");

			await ((ReactiveCommand<DbInfo, Unit>)page.DeleteDatabaseCommand).Execute(target);

			bool stillOnServer = await DatabaseExists("base_to_delete");
			Assert.That(stillOnServer, Is.False, "база должна быть удалена с сервера");
			Assert.That(page.Databases.Select(d => d.BaseName), Does.Not.Contain("base_to_delete"),
				"список на странице должен обновиться");
			Assert.That(page.Databases.Select(d => d.BaseName), Does.Contain("base_to_keep"));
			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Success,
				Arg.Is<string>(m => m.Contains("удалена")), Arg.Any<string>());
		}

		[Test(Description = "Отказ в диалоге подтверждения оставляет базу на месте")]
		public async Task DeleteDatabaseCommand_Declined_KeepsDatabase() {
			await SeedBaseWithAccess("base_survivor", "Выживший");
			Pages.AnswerNoToQuestions(); // на диалог подтверждения отвечаем «нет»

			var page = await Pages.OpenDatabasesPage(LoginAs());
			var target = page.Databases.First(d => d.BaseName == "base_survivor");

			await ((ReactiveCommand<DbInfo, Unit>)page.DeleteDatabaseCommand).Execute(target);

			Assert.That(await DatabaseExists("base_survivor"), Is.True, "отказ значит отказ");
		}

		[Test(Description = "При недоступной метабазе страница прогресса остаётся и объясняет, что синхронизировать некуда")]
		public async Task RefreshMetadataCommand_WithoutMetabase_KeepsPageAndExplains() {
			await CreateApplicationDatabase("base_any", "Любая");
			await DropMetabase(); // синхронизировать будет некуда
			try {
				var page = await Pages.OpenDatabasesPage(LoginAs());

				await page.RefreshMetadataCommand.Execute();
				var progress = await Pages.RunLastProgressPage();

				// со страницы отказа уходит сам пользователь: она появилась мгновение назад,
				// и уводить его посреди анимации перехода нельзя
				Assert.That(progress.IsFailed, Is.True);
				Assert.That(progress.FailureMessage, Does.Contain("QSLauncher"));
				Assert.That(Pages.PopCount, Is.Zero, "страница отказа сама не закрывается");
				Pages.InteractiveMessage.DidNotReceive().ShowMessage(ImportanceLevel.Success,
					Arg.Any<string>(), "Синхронизация метаинформации");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Закрыть на странице отказа возвращает на список баз")]
		public async Task RefreshMetadataCommand_CloseAfterFailure_ReturnsToDatabases() {
			await CreateApplicationDatabase("base_any", "Любая");
			await DropMetabase();
			try {
				var page = await Pages.OpenDatabasesPage(LoginAs());

				await page.RefreshMetadataCommand.Execute();
				var progress = await Pages.RunLastProgressPage();
				await progress.CloseCommand.Execute();

				Assert.That(Pages.PopCount, Is.EqualTo(1), "страницу прогресса должно снять");
			}
			finally {
				await DeployMetabase();
			}
		}

		[Test(Description = "Кнопка Подключиться отдаёт строку подключения запускателю программы")]
		public async Task ConnectCommand_PassesConnectionToAppRunner() {
			await SeedBaseWithAccess("base_to_run", "Рабочая");

			var page = await Pages.OpenDatabasesPage(LoginAs());
			page.SelectedDatabase = page.Databases.First(d => d.BaseName == "base_to_run"); // выбор базы в списке

			await page.ConnectAsync();

			Pages.AppRunner.Received(1).Run(Arg.Is<LoginToDatabaseResponse>(
				r => r.Success && r.ConnectionString.Contains("base_to_run")));
		}

		[Test(Description = "Кнопка Пользователи открывает страницу управления")]
		public async Task OpenUserManagementCommand_PushesUsersPage() {
			await SeedBaseWithAccess("base_users_page", "С пользователями");

			var page = await Pages.OpenDatabasesPage(LoginAs());

			await page.OpenUserManagementCommand.Execute();

			Assert.That(Pages.PushedPages, Has.Count.EqualTo(1));
			Assert.That(Pages.PushedPages[0], Is.InstanceOf<UsersVM>());
			Assert.That(((UsersVM)Pages.PushedPages[0]).CanManageUsers, Is.True,
				"страница должна получить рабочий провайдер");
		}
	}
}
