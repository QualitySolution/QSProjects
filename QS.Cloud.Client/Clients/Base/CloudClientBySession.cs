using Grpc.Core;
using System;

namespace QS.Cloud.Client
{
	public class CloudClientBySession : CloudClientServiceBase
	{
		protected readonly ISessionInfoProvider sessionInfoProvider;

		public CloudClientBySession(ISessionInfoProvider sessionInfoProvider, string serviceAddress, int servicePort)
			: base(serviceAddress, servicePort)
		{
			this.sessionInfoProvider = sessionInfoProvider ?? throw new ArgumentNullException(nameof(sessionInfoProvider));
			headers = new Metadata { { "Authorization", $"Bearer {this.sessionInfoProvider.SessionId}" } };
		}

		public override bool CanConnect => !String.IsNullOrEmpty(sessionInfoProvider.SessionId);
	}
}
