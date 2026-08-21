using Grpc.Core;
using NSubstitute;
using NUnit.Framework;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Creation;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using System;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Список баз, удаление, подключение и создание. Реестр баз ведёт облако, поэтому
	/// проверяем не состояние сервера, а то, о чём лаунчер попросил облако и как разобрал ответ.
	/// </summary>
	[TestFixture(TestOf = typeof(QSCloudProvider))]
	public class CloudProvider_DatabasesTest : CloudProviderTestFixtureBase {

		#region Наполнение базы - подменяем, реальный скрипт тут не нужен

		/// <summary>Ресурс-пустышка: важно только то, что фабрика по нему находит модель</summary>
		public sealed class FakeResources : DbCreationResources { }

		public sealed class FakeCreationModel : IDbCreatorModel {
			public static bool WasRun { get; set; }
			public static string ConnectionStringSeen { get; set; }
			public static bool Result { get; set; } = true;

			private readonly DbCreationResources resources;
			public FakeCreationModel(DbCreationResources resources) => this.resources = resources;

			public bool RunCreation(string dbName, string dbTitle) {
				WasRun = true;
				ConnectionStringSeen = resources.ConnectionString;
				return Result;
			}
		}

		private IDbCreatorInteraction interaction;

		[SetUp]
		public void ResetCreationModel() {
			FakeCreationModel.WasRun = false;
			FakeCreationModel.ConnectionStringSeen = null;
			FakeCreationModel.Result = true;
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

		#endregion

		[Test(Description = "Список баз приходит из облака со всеми полями")]
		public void GetUserDatabases_ReturnsBasesFromCloud() {
			var db = AddBase("cloud_base", "Облачная", version: "4.2");
			Grant(AdminLogin, db.Id);

			var databases = LoginAs().GetUserDatabases();

			var found = databases.FirstOrDefault(d => d.BaseName == "cloud_base");
			Assert.That(found, Is.Not.Null);
			Assert.That(found.Title, Is.EqualTo("Облачная"));
			Assert.That(found.Version, Is.EqualTo("4.2"));
			Assert.That(found.BaseId, Is.EqualTo(db.Id), "идентификатор базы ведёт облако");
		}

		[Test(Description = "Базы чужого продукта в список не попадают")]
		public void GetUserDatabases_ForeignProduct_NotListed() {
			AddBase("ours");
			AddBase("alien", product: OtherProductCode); // тот же аккаунт, другой продукт

			var names = LoginAs().GetUserDatabases().Select(d => d.BaseName).ToList();

			Assert.That(names, Does.Contain("ours"));
			Assert.That(names, Does.Not.Contain("alien"));
		}

		[Test(Description = "Обычный пользователь видит только базы с выданным доступом")]
		public void GetUserDatabases_PlainUser_SeesOnlyGranted() {
			var visible = AddBase("visible");
			AddBase("hidden"); // доступ на неё не выдаём
			AddUser("plain", "plain-pass");
			Grant("plain", visible.Id);

			var names = LoginAs("plain", "plain-pass").GetUserDatabases().Select(d => d.BaseName).ToList();

			Assert.That(names, Is.EquivalentTo(new[] { "visible" }));
		}

		[Test(Description = "Удаление базы убирает её из реестра облака вместе с доступами")]
		public void DropDatabase_RemovesBaseAndAccessFromCloud() {
			var db = AddBase("to_drop");
			AddUser("someone");
			Grant("someone", db.Id);

			bool dropped = LoginAs().DropDatabase(new DbInfo { BaseId = db.Id, BaseName = "to_drop" });

			Assert.That(dropped, Is.True);
			Assert.That(State.FindBase(db.Id), Is.Null, "запись о базе должна уйти из реестра");
			Assert.That(State.Access.Any(a => a.BaseId == db.Id), Is.False, "доступы удаляются вместе с базой");
		}

		[Test(Description = "Удаление несуществующей базы возвращает отказ, а не исключение")]
		public void DropDatabase_UnknownBase_ReturnsFalse() {
			bool dropped = LoginAs().DropDatabase(new DbInfo { BaseId = 9999, BaseName = "нет-такой" });

			Assert.That(dropped, Is.False);
		}

		[Test(Description = "Подключение к базе открывает сессию и отдаёт её идентификатор")]
		public void LoginToDatabase_OpensSessionAndReturnsSessionId() {
			var db = AddBase("to_connect", "Рабочая");
			Grant(AdminLogin, db.Id);

			var response = LoginAs().LoginToDatabase(new DbInfo { BaseId = db.Id, Title = "Рабочая" });

			Assert.That(response.Success, Is.True, response.ErrorMessage);
			Assert.That(response.ConnectionString, Does.Contain("to_connect"), "имя базы даёт облако");
			Assert.That(response.ConnectionString, Does.Contain("db.example"), "адрес сервера тоже от облака");
			Assert.That(response.Parameters["SessionId"], Is.EqualTo($"session-{db.Id}"),
				"идентификатор сессии - облачное понятие, у свободного подключения его нет");
			Assert.That(response.Parameters["BaseTitle"], Is.EqualTo("Рабочая"));
		}

		[Test(Description = "Отказ облака в сессии - Response с ошибкой, приложение не падает")]
		public void LoginToDatabase_CloudRefusesSession_ReturnsFailure() {
			var db = AddBase("locked_base");
			State.RefuseSessions = true;

			var response = LoginAs().LoginToDatabase(new DbInfo { BaseId = db.Id });

			Assert.That(response.Success, Is.False);
		}

		[Test(Description = "Создание базы: облако заводит запись и лаунчер её наполняет")]
		public void CreateDatabase_NewBase_RegistersInCloudAndFillsIt() {
			bool created = LoginAs().CreateDatabase(CreationRequest("fresh_base", "Свежая"));

			Assert.That(created, Is.True);
			Assert.That(State.Bases.Any(b => b.Name == "fresh_base"), Is.True, "база должна появиться в реестре");
			Assert.That(FakeCreationModel.WasRun, Is.True, "наполнение должно запуститься");
			Assert.That(FakeCreationModel.ConnectionStringSeen, Does.Contain("fresh_base"),
				"наполнение идёт по строке подключения из сессии облака");
		}

		[Test(Description = "Существующая база: «Пересоздать» сносит её и заводит заново")]
		public void CreateDatabase_Recreate_DropsAndCreatesAgain() {
			var existing = AddBase("busy_base");
			interaction.AskDropExistingDatabase("busy_base").Returns(ToDoWithExistingDatabase.Recreate);

			bool created = LoginAs().CreateDatabase(CreationRequest("busy_base"));

			Assert.That(created, Is.True);
			Assert.That(State.FindBase(existing.Id), Is.Null, "старая запись должна быть удалена");
			Assert.That(State.Bases.Any(b => b.Name == "busy_base"), Is.True, "и заведена новая");
		}

		[Test(Description = "Существующая база: «Перезаписать» чистит её, сохраняя запись и доступы")]
		public void CreateDatabase_Rewrite_ClearsButKeepsRegistryAndAccess() {
			var existing = AddBase("keep_base");
			AddUser("colleague");
			Grant("colleague", existing.Id);
			interaction.AskDropExistingDatabase("keep_base").Returns(ToDoWithExistingDatabase.Rewrite);

			bool created = LoginAs().CreateDatabase(CreationRequest("keep_base"));

			Assert.That(created, Is.True);
			Assert.That(State.FindBase(existing.Id), Is.Not.Null, "запись о базе сохраняется");
			Assert.That(State.FindBase(existing.Id).HasData, Is.False, "но сама база очищена");
			Assert.That(State.FindAccess("colleague", existing.Id)?.HasAccess, Is.True,
				"права доступа коллег при перезаписи не теряются");
		}

		[Test(Description = "Существующая база: «Ничего не делать» ничего и не делает")]
		public void CreateDatabase_Nothing_LeavesEverythingAlone() {
			var existing = AddBase("untouched");
			interaction.AskDropExistingDatabase("untouched").Returns(ToDoWithExistingDatabase.Nothing);

			bool created = LoginAs().CreateDatabase(CreationRequest("untouched"));

			Assert.That(created, Is.False);
			Assert.That(State.FindBase(existing.Id), Is.Not.Null);
			Assert.That(FakeCreationModel.WasRun, Is.False, "наполнение запускаться не должно");
		}

		[Test(Description = "Нет прав администратора базы - наполнение не начинается, пользователь получает объяснение")]
		public void CreateDatabase_SessionWithoutAdmin_ReportsErrorAndStops() {
			State.SessionWithoutAdmin = true; // облако открыло сессию, но без прав на наполнение

			bool created = LoginAs().CreateDatabase(CreationRequest("no_rights_base"));

			Assert.That(created, Is.False);
			Assert.That(FakeCreationModel.WasRun, Is.False);
			interaction.Received().ReportError(
				Arg.Is<string>(m => m.Contains("прав администратора")), Arg.Any<string>());
		}

		[Test(Description = "Облако не открыло сессию - тоже объяснение, а не падение")]
		public void CreateDatabase_SessionRefused_ReportsError() {
			State.RefuseSessions = true;

			bool created = LoginAs().CreateDatabase(CreationRequest("no_session_base"));

			Assert.That(created, Is.False);
			interaction.Received().ReportError(
				Arg.Is<string>(m => m.Contains("сессию")), Arg.Any<string>());
		}

		[Test(Description = "Пустой запрос на создание - нарушение контракта, а не ответ пользователю")]
		public void CreateDatabase_NullRequest_Throws() {
			Assert.Throws<ArgumentNullException>(() => LoginAs().CreateDatabase(null));
		}

		[Test(Description = "Сбой облака при создании превращается в исключение с текстом сервера")]
		public void CreateDatabase_CloudFailure_ThrowsWithServerDetail() {
			var provider = LoginAs();
			BreakCloud(StatusCode.Internal, "реестр баз недоступен");

			var exception = Assert.Throws<InvalidOperationException>(
				() => provider.CreateDatabase(CreationRequest("broken_base")));

			Assert.That(exception.Message, Does.Contain("реестр баз недоступен"));
		}
	}
}
