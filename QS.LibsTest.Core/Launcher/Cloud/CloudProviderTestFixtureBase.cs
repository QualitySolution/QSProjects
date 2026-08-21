using NUnit.Framework;
using QS.Cloud.Client;
using QS.Cloud.Client.DataBase;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QS.Launcher.Test.Cloud {
	[NonParallelizable]
	public abstract class CloudProviderTestFixtureBase {
		protected const string AccountName = "testaccount";
		protected const string AdminLogin = "admin";
		protected const string AdminPassword = "admin-pass";
		protected const byte TestProductCode = 1;
		protected const byte OtherProductCode = 77;

		private readonly List<IDisposable> createdProviders = new List<IDisposable>();
		private Stopwatch testTimer;

		protected FakeCloudBackend Cloud { get; private set; }
		protected FakeCloudBackend.CloudState State => Cloud.State;

		#region Жизненный цикл

		[SetUp]
		public virtual void StartCloud() {
			testTimer = Stopwatch.StartNew();
			var test = TestContext.CurrentContext.Test;
			Log($"┌─ {test.ClassName?.Split('.').Last()}.{test.Name}");
			if(test.Properties.Get("Description") is string description)
				Log($"│  {description}");

			Cloud = new FakeCloudBackend();

			CloudClientServiceBase.OverrideAddress = "127.0.0.1";
			CloudClientServiceBase.OverridePort = Cloud.Port;
			CloudClientServiceBase.UseInsecureOverride = true;

			State.AddUser(AdminLogin, AdminPassword, isAdmin: true, name: "Администратор");
			LogStep("облако поднято на порту {0}, в нём администратор {1}", Cloud.Port, AdminLogin);
		}

		[TearDown]
		public virtual void StopCloud() {
			foreach(var provider in createdProviders)
				provider.Dispose();
			createdProviders.Clear();

			var result = TestContext.CurrentContext.Result;
			if(result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
				DumpCloudState();

			Log($"└─ {result.Outcome.Status}, {testTimer?.ElapsedMilliseconds ?? 0} мс");

			Cloud?.Dispose();
			Cloud = null;
		}

		private static void Log(string line) => TestContext.Out.WriteLine(line);

		protected static void LogStep(string format, params object[] args) =>
			Log("  · " + (args.Length == 0 ? format : string.Format(format, args)));

		/// <summary>Снимок облака на момент падения - иначе по одному Assert не понять, что осталось</summary>
		private void DumpCloudState() {
			Log("│  СОСТОЯНИЕ ОБЛАКА НА МОМЕНТ ПАДЕНИЯ:");
			foreach(var user in State.Users)
				Log($"│    пользователь: {user.Info.Login} «{user.Info.Name}» "
					+ $"админ={user.Info.IsAccountAdmin} отключён={user.Info.Disabled}");
			foreach(var db in State.Bases)
				Log($"│    база: id={db.Id} {db.Name} «{db.Title}» продукт={db.ProductId} наполнена={db.HasData}");
			foreach(var access in State.Access)
				Log($"│    доступ: {access.Login} -> база {access.BaseId} "
					+ $"есть={access.HasAccess} админ={access.Admin} чтение={access.ReadOnly}");
			if(State.Users.Count == 0 && State.Bases.Count == 0)
				Log("│    пусто");
		}

		#endregion

		#region Провайдер

		protected QSCloudProvider CreateProvider(string login = AdminLogin, string password = AdminPassword,
			byte productCode = TestProductCode) {
			var parameters = new List<ConnectionParameterValue> {
				new ConnectionParameterValue(new ConnectionParameter("Account", "Аккаунт"), AccountName),
				new ConnectionParameterValue(new ConnectionParameter("Login", "Пользователь"), login)
			};

			var provider = new QSCloudProvider(parameters, productCode, password);
			createdProviders.Add(provider);
			LogStep("собран провайдер: {0}\\{1}, продукт {2}", AccountName, login, productCode);
			return provider;
		}

		protected QSCloudProvider LoginAs(string login = AdminLogin, string password = AdminPassword,
			byte productCode = TestProductCode) {
			var provider = CreateProvider(login, password, productCode);
			var response = provider.LoginToServer();
			Assert.That(response.Success, Is.True, $"Не удалось войти в облако как {login}: {response.ErrorMessage}");
			LogStep("вход выполнен как {0}: админ={1}, создание баз={2}, управление пользователями={3}",
				login, provider.IsAdmin, provider.CanCreateDatabase, provider.CanManageUsers);
			return provider;
		}

		#endregion

		#region Подготовка состояния

		protected FakeCloudBackend.CloudBase AddBase(string name, string title = null,
			byte product = TestProductCode, string version = "1.0") {
			var db = State.AddBase(name, title, product, version);
			LogStep("в облаке заведена база {0} «{1}» (id {2}, продукт {3})", name, title ?? name, db.Id, product);
			return db;
		}

		protected FakeCloudBackend.CloudUser AddUser(string login, string password = "pass-1234",
			bool isAdmin = false, string name = null, bool disabled = false) {
			var user = State.AddUser(login, password, isAdmin, name, disabled: disabled);
			LogStep("в облаке заведён пользователь {0} (админ: {1}, отключён: {2})", login, isAdmin, disabled);
			return user;
		}

		protected void Grant(string login, int baseId, bool admin = false, bool readOnly = false) {
			State.Grant(login, baseId, admin, readOnly);
			LogStep("в облаке выдан доступ: {0} -> база {1} (админ: {2}, только чтение: {3})",
				login, baseId, admin, readOnly);
		}

		/// <summary>Облако начинает отвечать отказом на любой вызов</summary>
		protected void BreakCloud(Grpc.Core.StatusCode code, string detail = null) {
			State.FailEverythingWith = code;
			State.FailureDetail = detail;
			LogStep("облако переведено в отказ: {0} «{1}»", code, detail ?? "-");
		}

		protected void RepairCloud() {
			State.FailEverythingWith = null;
			State.FailureDetail = null;
		}

		#endregion
	}
}
