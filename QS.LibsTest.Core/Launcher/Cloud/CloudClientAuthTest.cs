using Grpc.Core;
using NUnit.Framework;
using QS.Cloud.Client;
using System;
using System.Text;

namespace QS.Launcher.Test.Cloud {
	/// <summary>Сборка запроса к облак</summary>
	[TestFixture(TestOf = typeof(CloudClientByBasicAuth))]
	public class CloudClientAuthTest
	{
		private sealed class Probe : CloudClientByBasicAuth {
			public Probe(IBasicAuthInfoProvider auth, string address, int port) : base(auth, address, port) { }

			public string AuthorizationHeader => headers.GetValue("authorization");
			public string ChannelTarget => Channel.Target;
		}

		[Test(Description = "Логин с аккаунтом и пароль уезжают заголовком Basic в base64")]
		public void BasicAuth_PacksAccountLoginAndPassword() {
			using(var probe = NewProbe(new BasicAuthInfoProvider(@"testaccount\admin", "s3cret"))) {
				string header = probe.AuthorizationHeader;

				Assert.That(header, Does.StartWith("Basic "));
				Assert.That(Decode(header), Is.EqualTo(@"testaccount\admin:s3cret"),
					"облако разбирает пару как «аккаунт\\логин:пароль»");
			}
		}

		[Test(Description = "После смены пароля клиент подписывается новым - со старым облако ответит Unauthenticated")]
		public void UpdatePassword_RebuildsAuthorizationHeader() {
			using(var probe = NewProbe(new BasicAuthInfoProvider(@"testaccount\admin", "old"))) {
				probe.UpdatePassword("n3w");

				Assert.That(Decode(probe.AuthorizationHeader), Is.EqualTo(@"testaccount\admin:n3w"));
			}
		}

		private static string Decode(string authorizationHeader) =>
			Encoding.UTF8.GetString(
				Convert.FromBase64String(authorizationHeader.Substring("Basic ".Length)));

		private static Probe NewProbe(IBasicAuthInfoProvider auth) =>
			new Probe(auth, "core.cloud.qsolution.ru", 443);
	}
}
