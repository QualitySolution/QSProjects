using Grpc.Core;
using System;
using System.Text;

namespace QS.Cloud.Client
{
	public class CloudClientByBasicAuth : CloudClientServiceBase
	{
		public CloudClientByBasicAuth(IBasicAuthInfoProvider basicAuthInfoProvider, string serviceAddress, int servicePort)
			: base(serviceAddress, servicePort)
		{
			headers = new Metadata
			{ {
				"Authorization",
				$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicAuthInfoProvider.UserName}:{basicAuthInfoProvider.Password}"))}"
			} };
			
		}

		public override bool CanConnect => throw new NotImplementedException();
	}
}
