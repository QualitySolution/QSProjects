using Grpc.Core;
using System;
using System.Text;

namespace QS.Cloud.Client
{
	public class CloudClientByBasicAuth : CloudClientServiceBase {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>Столько ждём соединения в CanConnect - это проверка доступности, а не сама операция</summary>
		private const int ConnectTimeoutSeconds = 5;

		private readonly IBasicAuthInfoProvider authInfo;

		public CloudClientByBasicAuth(IBasicAuthInfoProvider basicAuthInfoProvider, string serviceAddress, int servicePort)
			: base(serviceAddress, servicePort) {
			authInfo = basicAuthInfoProvider ?? throw new ArgumentNullException(nameof(basicAuthInfoProvider));
			headers = BuildHeaders();
		}

		private Metadata BuildHeaders() => new Metadata
		{ {
			"Authorization",
			$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authInfo.UserName}:{authInfo.Password}"))}"
		} };

		public virtual void UpdatePassword(string newPassword) {
			authInfo.UpdatePassword(newPassword);
			headers = BuildHeaders();
		}

		public override bool CanConnect { get {
				try {
					Channel.ConnectAsync(DateTime.UtcNow.AddSeconds(ConnectTimeoutSeconds))
						.GetAwaiter().GetResult();
					return true;
				}
				catch {
					return false;
				}
			}
		}
	}
}
