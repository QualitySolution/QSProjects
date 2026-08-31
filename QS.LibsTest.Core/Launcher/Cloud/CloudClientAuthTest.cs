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
				string decoded = Encoding.UTF8.GetString(
					Convert.FromBase64String(header.Substring("Basic ".Length)));
				Assert.That(decoded, Is.EqualTo(@"testaccount\admin:s3cret"),
					"облако разбирает пару как «аккаунт\\логин:пароль»");
			}
		}

		private static Probe NewProbe(IBasicAuthInfoProvider auth) =>
			new Probe(auth, "core.cloud.qsolution.ru", 443);
	}
}
