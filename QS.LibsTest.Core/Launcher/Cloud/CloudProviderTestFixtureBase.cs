using Grpc.Core;
using NSubstitute;
using NUnit.Framework;
using QS.Cloud.Client;
using QS.Cloud.Client.Clients;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Облачный провайдер проверяется по входу и выходу: три клиента приходят подстановками,
	/// gRPC-сервер не поднимается. Раньше на его месте стоял рукописный FakeCloudBackend -
	/// он воспроизводил правила облака (кто админ, какие базы видны), и тесты в итоге
	/// проверяли эту копию, а не наш код. Здесь проверяется только то, за что отвечает
	/// провайдер: что он послал облаку и как разобрал ответ.
	/// </summary>
	public abstract class CloudProviderTestFixtureBase {
		protected const string AccountName = "testaccount";
		protected const string AdminLogin = "admin";
		protected const byte TestProductCode = 1;
		protected const byte OtherProductCode = 77;

		protected LoginManagementCloudClient LoginClient { get; private set; }
		protected DataBaseManagementCloudClient DbClient { get; private set; }
		protected UserManagementCloudClient UserClient { get; private set; }

		[SetUp]
		public void CreateClients() {
			var auth = new BasicAuthInfoProvider($@"{AccountName}\{AdminLogin}", "pass");

			LoginClient = Substitute.For<LoginManagementCloudClient>(auth);
			DbClient = Substitute.For<DataBaseManagementCloudClient>(auth, (uint)TestProductCode);
			UserClient = Substitute.For<UserManagementCloudClient>(auth);

			// канал до облака поднимается лениво и в тестах не нужен, но CanCreateDatabase
			// про него спрашивает - отвечаем, что связь есть
			DbClient.CanConnect.Returns(true);
		}

		protected QSCloudProvider CreateProvider(string login = AdminLogin, byte productCode = TestProductCode) =>
			new QSCloudProvider(AccountName, login, productCode, LoginClient, DbClient, UserClient);

		/// <summary>Провайдер после успешного входа - состояние, из которого работают все страницы лаунчера</summary>
		protected QSCloudProvider LoginAs(string login = AdminLogin, bool isAdmin = true, bool needUpdate = false) {
			LoginClient.Start(Arg.Any<string>())
				.Returns(new StartResponse { YouAccountAdmin = isAdmin, NeedUpdateLauncher = needUpdate });

			var provider = CreateProvider(login);
			var response = provider.LoginToServer();
			Assert.That(response.Success, Is.True, response.ErrorMessage);
			return provider;
		}

		/// <summary>Отказ облака - то, из чего провайдер обязан сделать понятное сообщение</summary>
		protected static RpcException Refusal(StatusCode code, string detail = "облако недоступно") =>
			new RpcException(new Status(code, detail));

		protected static BaseInfo Base(int id, string name, string title = null, string version = "1.0") =>
			new BaseInfo { BaseId = id, BaseName = name, BaseTitle = title ?? name, BaseVersion = version };

		protected static UserInfo User(string login, string name = "", bool isAdmin = false, bool disabled = false) =>
			new UserInfo {
				Login = login, Name = name, Email = string.Empty, Phone = string.Empty,
				Post = string.Empty, Comment = string.Empty, Disabled = disabled, IsAccountAdmin = isAdmin
			};
	}
}
