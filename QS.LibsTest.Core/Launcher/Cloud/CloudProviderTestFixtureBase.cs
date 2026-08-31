using Grpc.Core;
using NSubstitute;
using NUnit.Framework;
using QS.Cloud.Client;
using QS.Cloud.Client.Clients;
using QS.Cloud.Client.DataBase;
using QS.Cloud.Core;

namespace QS.Launcher.Test.Cloud {
	public abstract class CloudProviderTestFixtureBase {
		protected const string AccountName = "testaccount";
		protected const string AdminLogin = "admin";
		protected const byte TestProductCode = 1;

		protected LoginManagementCloudClient LoginClient { get; private set; }
		protected DataBaseManagementCloudClient DbClient { get; private set; }
		protected UserManagementCloudClient UserClient { get; private set; }

		[SetUp]
		public void CreateClients() {
			var auth = new BasicAuthInfoProvider($@"{AccountName}\{AdminLogin}", "pass");

			LoginClient = Substitute.For<LoginManagementCloudClient>(auth);
			DbClient = Substitute.For<DataBaseManagementCloudClient>(auth, (uint)TestProductCode);
			UserClient = Substitute.For<UserManagementCloudClient>(auth);

			DbClient.CanConnect.Returns(true);
		}

		protected QSCloudProvider CreateProvider(string login = AdminLogin, byte productCode = TestProductCode) =>
			new QSCloudProvider(AccountName, login, productCode, LoginClient, DbClient, UserClient);

		protected QSCloudProvider LoginAs(string login = AdminLogin, bool isAdmin = true) {
			LoginClient.Start(Arg.Any<string>())
				.Returns(new StartResponse { YouAccountAdmin = isAdmin });

			var provider = CreateProvider(login);
			var response = provider.LoginToServer();
			Assert.That(response.Success, Is.True, response.ErrorMessage);
			return provider;
		}

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
