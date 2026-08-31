using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;
using QS.DbManagement.Creation;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Список баз, подключение и создание
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_DatabasesTest : CloudProviderTestFixtureBase {

		public sealed class FakeResources : DbCreationResources { }

		public sealed class FakeCreationModel : IDbCreatorModel {
			public static bool WasRun { get; set; }
			public static string ConnectionStringSeen { get; set; }

			private readonly DbCreationResources resources;
			public FakeCreationModel(DbCreationResources resources) => this.resources = resources;

			public bool RunCreation(string dbName, string dbTitle) {
				WasRun = true;
				ConnectionStringSeen = resources.ConnectionString;
				return true;
			}
		}

		private IDbCreatorInteraction interaction;

		[SetUp]
		public void ResetCreationModel() {
			FakeCreationModel.WasRun = false;
			FakeCreationModel.ConnectionStringSeen = null;
			interaction = Substitute.For<IDbCreatorInteraction>();
		}

		private DbCreationRequest CreationRequest(string dbName, string title = null) {
			var map = new DbResourcesCreationMap();
			map.Register(typeof(FakeResources), typeof(FakeCreationModel));

			return new DbCreationRequest {
				DbName = dbName,
				DbTitle = title ?? dbName,
				Interaction = interaction,
				CreationFactory = new DbCreationFactory(map),
				CreationResources = new FakeResources()
			};
		}

		private void SessionOpens(int baseId, string baseName, bool isAdmin = true) =>
			LoginClient.StartSession(baseId).Returns(new StartSessionResponse {
				Success = true, SessionId = $"session-{baseId}", IsAdmin = isAdmin,
				Db = new BaseConnection {
					Login = "db_user", Password = "db_pass", Server = "db.example",
					BaseName = baseName, Port = 3306
				}
			});

		[Test(Description = "Список баз приходит из облака со всеми полями")]
		public void GetUserDatabases_MapsBasesFromCloud() {
			LoginClient.GetBasesForUser(TestProductCode)
				.Returns(new List<BaseInfo> { Base(3, "cloud_base", "Облачная", "4.2") });

			var found = LoginAs().GetUserDatabases().Single();

			Assert.That(found.BaseName, Is.EqualTo("cloud_base"));
			Assert.That(found.Title, Is.EqualTo("Облачная"));
			Assert.That(found.Version, Is.EqualTo("4.2"));
			Assert.That(found.BaseId, Is.EqualTo(3), "идентификатор базы ведёт облако");
		}

		[Test(Description = "Удаление базы отправляет её идентификатор в облако")]
		public void DropDatabase_SendsBaseIdToCloud() {
			DbClient.DropDataBase(5).Returns(new DropDataBaseResponse { Success = true });

			bool dropped = LoginAs().DropDatabase(new DbInfo { BaseId = 5, BaseName = "to_drop" });

			Assert.That(dropped, Is.True);
			DbClient.Received(1).DropDataBase(5);
		}

		[Test(Description = "Подключение отдаёт строку из облака и идентификатор сессии")]
		public void LoginToDatabase_ReturnsCloudConnectionAndSessionId() {
			SessionOpens(4, "to_connect");

			var response = LoginAs().LoginToDatabase(new DbInfo { BaseId = 4, Title = "Рабочая" });

			Assert.That(response.Success, Is.True, response.ErrorMessage);
			Assert.That(response.ConnectionString, Does.Contain("to_connect"), "имя базы даёт облако");
			Assert.That(response.ConnectionString, Does.Contain("db.example"), "адрес сервера тоже от облака");
			Assert.That(response.Parameters["SessionId"], Is.EqualTo("session-4"),
				"идентификатор сессии - облачное понятие, у свободного подключения его нет");
			Assert.That(response.Parameters["BaseTitle"], Is.EqualTo("Рабочая"));
		}

		[Test(Description = "Отказ облака в сессии - Response с ошибкой, приложение не падает")]
		public void LoginToDatabase_SessionRefused_ReturnsFailure() {
			LoginClient.StartSession(Arg.Any<int>())
				.Returns(new StartSessionResponse { Success = false, Description = "Сессии недоступны" });

			var response = LoginAs().LoginToDatabase(new DbInfo { BaseId = 6 });

			Assert.That(response.Success, Is.False);
		}

		[Test(Description = "Новой базы в облаке нет - заводим запись и наполняем")]
		public void CreateDatabase_NewBase_RegistersInCloudAndFillsIt() {
			DbClient.CheckDataBaseExists("fresh_base").Returns(new CheckDataBaseExistsResponse { Exists = false });
			DbClient.CreateDataBase("fresh_base", "Свежая").Returns(new CreateDataBaseResponse { BaseId = 11 });
			SessionOpens(11, "fresh_base");

			bool created = LoginAs().CreateDatabase(CreationRequest("fresh_base", "Свежая"));

			Assert.That(created, Is.True);
			DbClient.Received(1).CreateDataBase("fresh_base", "Свежая");
			Assert.That(FakeCreationModel.WasRun, Is.True, "наполнение должно запуститься");
			Assert.That(FakeCreationModel.ConnectionStringSeen, Does.Contain("fresh_base"),
				"наполнение идёт по строке подключения из сессии облака");
		}

		[Test(Description = "Пересоздать сносит запись в облаке и заводит новую")]
		public void CreateDatabase_Recreate_DropsThenCreates() {
			DbClient.CheckDataBaseExists("busy_base").Returns(new CheckDataBaseExistsResponse { Exists = true, BaseId = 12 });
			DbClient.DropDataBase(12).Returns(new DropDataBaseResponse { Success = true });
			DbClient.CreateDataBase("busy_base", "busy_base").Returns(new CreateDataBaseResponse { BaseId = 13 });
			SessionOpens(13, "busy_base");
			interaction.AskDropExistingDatabase("busy_base").Returns(ToDoWithExistingDatabase.Recreate);

			Assert.That(LoginAs().CreateDatabase(CreationRequest("busy_base")), Is.True);

			Received.InOrder(() => {
				DbClient.DropDataBase(12);
				DbClient.CreateDataBase("busy_base", "busy_base");
			});
		}

		[Test(Description = "Перезаписать чистит базу, сохраняя запись реестра и доступы")]
		public void CreateDatabase_Rewrite_ClearsAndKeepsRegistry() {
			DbClient.CheckDataBaseExists("keep_base").Returns(new CheckDataBaseExistsResponse { Exists = true, BaseId = 14 });
			DbClient.ClearDataBase(14).Returns(new ClearDataBaseResponse { Success = true });
			SessionOpens(14, "keep_base");
			interaction.AskDropExistingDatabase("keep_base").Returns(ToDoWithExistingDatabase.Rewrite);

			Assert.That(LoginAs().CreateDatabase(CreationRequest("keep_base")), Is.True);

			DbClient.Received(1).ClearDataBase(14);
			DbClient.DidNotReceive().DropDataBase(Arg.Any<int>());
			DbClient.DidNotReceive().CreateDataBase(Arg.Any<string>(), Arg.Any<string>());
		}

		[Test(Description = "Ничего не делать")]
		public void CreateDatabase_Nothing_LeavesEverythingAlone() {
			DbClient.CheckDataBaseExists("untouched").Returns(new CheckDataBaseExistsResponse { Exists = true, BaseId = 15 });
			interaction.AskDropExistingDatabase("untouched").Returns(ToDoWithExistingDatabase.Nothing);

			Assert.That(LoginAs().CreateDatabase(CreationRequest("untouched")), Is.False);

			DbClient.DidNotReceive().DropDataBase(Arg.Any<int>());
			DbClient.DidNotReceive().ClearDataBase(Arg.Any<int>());
			Assert.That(FakeCreationModel.WasRun, Is.False, "наполнение запускаться не должно");
		}

		[Test(Description = "Не удалось очистить базу - пользователь получает объяснение, наполнение не идёт")]
		public void CreateDatabase_ClearFailed_ReportsErrorAndStops() {
			DbClient.CheckDataBaseExists("stuck").Returns(new CheckDataBaseExistsResponse { Exists = true, BaseId = 16 });
			DbClient.ClearDataBase(16).Returns(new ClearDataBaseResponse { Success = false });
			interaction.AskDropExistingDatabase("stuck").Returns(ToDoWithExistingDatabase.Rewrite);

			Assert.That(LoginAs().CreateDatabase(CreationRequest("stuck")), Is.False);

			interaction.Received().ReportError(Arg.Is<string>(m => m.Contains("очистить")), Arg.Any<string>());
			Assert.That(FakeCreationModel.WasRun, Is.False);
		}

		[Test(Description = "Нет прав администратора базы - наполнение не начинается, пользователь получает объяснение")]
		public void CreateDatabase_SessionWithoutAdmin_ReportsErrorAndStops() {
			DbClient.CheckDataBaseExists("no_rights").Returns(new CheckDataBaseExistsResponse { Exists = false });
			DbClient.CreateDataBase(Arg.Any<string>(), Arg.Any<string>()).Returns(new CreateDataBaseResponse { BaseId = 17 });
			SessionOpens(17, "no_rights", isAdmin: false);

			Assert.That(LoginAs().CreateDatabase(CreationRequest("no_rights")), Is.False);

			interaction.Received().ReportError(
				Arg.Is<string>(m => m.Contains("прав администратора")), Arg.Any<string>());
			Assert.That(FakeCreationModel.WasRun, Is.False);
		}

		[Test(Description = "Облако не открыло сессию - тоже объяснение")]
		public void CreateDatabase_SessionRefused_ReportsError() {
			DbClient.CheckDataBaseExists("no_session").Returns(new CheckDataBaseExistsResponse { Exists = false });
			DbClient.CreateDataBase(Arg.Any<string>(), Arg.Any<string>()).Returns(new CreateDataBaseResponse { BaseId = 18 });
			LoginClient.StartSession(18)
				.Returns(new StartSessionResponse { Success = false, Description = "сессия недоступна" });

			Assert.That(LoginAs().CreateDatabase(CreationRequest("no_session")), Is.False);

			interaction.Received().ReportError(Arg.Is<string>(m => m.Contains("сессию")), Arg.Any<string>());
		}

		[Test(Description = "Пустой запрос на создание - нарушение контракта, а не ответ пользователю")]
		public void CreateDatabase_NullRequest_Throws() {
			var provider = LoginAs();

			Assert.Throws<ArgumentNullException>(() => provider.CreateDatabase(null));
		}

		[Test(Description = "Сбой облака при создании превращается в исключение с текстом сервера")]
		public void CreateDatabase_CloudFailure_ThrowsWithServerDetail() {
			DbClient.CheckDataBaseExists(Arg.Any<string>())
				.Throws(Refusal(StatusCode.Internal, "реестр баз недоступен"));
			var provider = LoginAs();

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.CreateDatabase(CreationRequest("broken_base")));

			Assert.That(exception.Message, Does.Contain("реестр баз недоступен"));
		}
	}
}
