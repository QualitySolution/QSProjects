using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Здесь проверяется только то, чем облако отличается от свободного подключения.
	/// Сами страницы про провайдер ничего не знают - их поведение (список баз, удаление
	/// с подтверждением, форма пользователя, выдача доступа) проверено один раз
	/// в DataBasesPage_IntegrationTest и UsersPage_IntegrationTest.
	/// </summary>
	[TestFixture(TestOf = typeof(DataBasesVM))]
	public class CloudPages_IntegrationTest : CloudProviderTestFixtureBase {
		private LauncherPagesHarness Pages { get; set; }

		[OneTimeSetUp]
		public void SetUpSchedulers() => LauncherPagesHarness.UseImmediateSchedulers();

		[SetUp]
		public void SetUpPages() =>
			Pages = new LauncherPagesHarness(new QsCloudConnectionTypeBase(), "Облако", TestProductCode);

		[Test(Description = "Синхронизация метаинформации в облаке недоступна - реестр баз ведёт оно само")]
		public async Task Capabilities_Cloud_HidesRefreshMetadata() {
			LoginClient.GetBasesForUser(Arg.Any<uint>()).Returns(new List<BaseInfo> { Base(1, "caps_base") });

			var page = await Pages.OpenDatabasesPage(LoginAs());

			Assert.That(page.Capabilities.CanRefreshMetadata, Is.False);
			// остальные права проверены на свободном подключении, здесь довольно того,
			// что страница вообще собралась поверх облачного провайдера
			Assert.That(page.Capabilities.CanCreate, Is.True);
		}

		[Test(Description = "Кнопка «Подключиться» отдаёт запускателю идентификатор сессии - у свободного подключения его нет")]
		public async Task ConnectCommand_PassesCloudSessionToAppRunner() {
			LoginClient.GetBasesForUser(Arg.Any<uint>()).Returns(new List<BaseInfo> { Base(2, "to_run", "Рабочая") });
			LoginClient.StartSession(2).Returns(new StartSessionResponse {
				Success = true, SessionId = "session-2", IsAdmin = true,
				Db = new BaseConnection {
					Login = "db_user", Password = "db_pass", Server = "db.example", BaseName = "to_run", Port = 3306
				}
			});

			var page = await Pages.OpenDatabasesPage(LoginAs());
			page.SelectedDatabase = page.Databases.First(d => d.BaseName == "to_run");

			await page.ConnectAsync();

			Pages.AppRunner.Received(1).Run(Arg.Is<LoginToDatabaseResponse>(
				r => r.Success && r.Parameters["SessionId"] == "session-2"));
		}

		[Test(Description = "Недоступное облако страница показывает сообщением, а не падением")]
		public void UsersPage_CloudUnavailable_ShowsError() {
			var provider = LoginAs();
			UserClient.GetUsers().Throws(Refusal(Grpc.Core.StatusCode.Unavailable));

			Assert.DoesNotThrowAsync(async () => await Pages.OpenUsersPage(provider));

			Pages.InteractiveMessage.Received().ShowMessage(ImportanceLevel.Error,
				Arg.Any<string>(), "Управление пользователями");
		}
	}
}
