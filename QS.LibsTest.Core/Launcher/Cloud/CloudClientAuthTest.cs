using Grpc.Core;
using NUnit.Framework;
using QS.Cloud.Client;
using System;
using System.Text;

namespace QS.Launcher.Test.Cloud {
	/// <summary>
	/// Сборка запроса к облаку - то немногое, что раньше проверялось только тем, что фейковый
	/// сервер соглашался нас пустить. Ломается такое тихо: заголовок уедет неверный, и облако
	/// ответит «неверный логин или пароль», хотя пароль правильный.
	/// </summary>
	[TestFixture(TestOf = typeof(CloudClientByBasicAuth))]
	public class CloudClientAuthTest {

		/// <summary>Даёт добраться до защищённых членов клиента, ничего не меняя в его поведении</summary>
		private sealed class Probe : CloudClientByBasicAuth {
			public Probe(IBasicAuthInfoProvider auth, string address, int port) : base(auth, address, port) { }

			public string AuthorizationHeader => headers.GetValue("authorization");
			public string ChannelTarget => Channel.Target;
		}

		private string savedAddress;
		private int? savedPort;

		[SetUp]
		public void SaveOverrides() {
			savedAddress = CloudClientServiceBase.OverrideAddress;
			savedPort = CloudClientServiceBase.OverridePort;
		}

		[TearDown]
		public void RestoreOverrides() {
			CloudClientServiceBase.OverrideAddress = savedAddress;
			CloudClientServiceBase.OverridePort = savedPort;
		}

		[Test(Description = "Логин с аккаунтом и пароль уезжают заголовком Basic в base64")]
		public void BasicAuth_PacksAccountLoginAndPassword() {
			using(var probe = NewProbe(new BasicAuthInfoProvider(@"testaccount\admin", "s3cret"))) {
				string header = probe.AuthorizationHeader;

				Assert.That(header, Does.StartWith("Basic "));
				string decoded = Encoding.UTF8.GetString(
					Convert.FromBase64String(header.Substring("Basic ".Length)));
				Assert.That(decoded, Is.EqualTo(@"testaccount\admin:s3cret"),
					"облако разбирает пару как «аккаунт\\логин:пароль»");
			}
		}

		[Test(Description = "Пустой пароль заголовок не ломает - облако должно ответить отказом, а не мы")]
		public void BasicAuth_EmptyPassword_StillWellFormed() {
			using(var probe = NewProbe(new BasicAuthInfoProvider(@"acc\user", null))) {
				string decoded = Encoding.UTF8.GetString(
					Convert.FromBase64String(probe.AuthorizationHeader.Substring("Basic ".Length)));

				Assert.That(decoded, Is.EqualTo(@"acc\user:"));
			}
		}

		[Test(Description = "Заданный override уводит клиента на локальный адрес и порт")]
		public void Override_RedirectsChannelToLocalServer() {
			CloudClientServiceBase.OverrideAddress = "127.0.0.1";
			CloudClientServiceBase.OverridePort = 5555;

			using(var probe = NewProbe(new BasicAuthInfoProvider(@"acc\user", "pass"))) {
				Assert.That(probe.ChannelTarget, Is.EqualTo("127.0.0.1:5555"));
			}
		}

		[Test(Description = "Без override клиент идёт на адрес, заданный самим сервисом")]
		public void WithoutOverride_UsesServiceAddress() {
			CloudClientServiceBase.OverrideAddress = null;
			CloudClientServiceBase.OverridePort = null;

			using(var probe = NewProbe(new BasicAuthInfoProvider(@"acc\user", "pass"))) {
				Assert.That(probe.ChannelTarget, Is.EqualTo("core.cloud.qsolution.ru:443"));
			}
		}

		private static Probe NewProbe(IBasicAuthInfoProvider auth) =>
			new Probe(auth, "core.cloud.qsolution.ru", 443);
	}
}
