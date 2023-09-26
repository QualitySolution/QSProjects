using System;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public class CloudClientServiceBase : IDisposable
	{
		private readonly ISessionInfoProvider sessionInfoProvider;
		private readonly string serviceAddress;
		private readonly int servicePort;

		protected readonly Metadata Headers;
        
		public CloudClientServiceBase(ISessionInfoProvider sessionInfoProvider, string serviceAddress, int servicePort)
		{
			this.sessionInfoProvider = sessionInfoProvider ?? throw new ArgumentNullException(nameof(sessionInfoProvider));
			this.serviceAddress = serviceAddress;
			this.servicePort = servicePort;
			Headers = new Metadata {{"Authorization", $"Bearer {this.sessionInfoProvider.SessionId}"}};
		}

		private Channel channel;
		protected Channel Channel {
			get {
				if(channel == null || channel.State == ChannelState.Shutdown)
					channel = new Channel(serviceAddress, servicePort, ChannelCredentials.Insecure);
				if (channel.State == ChannelState.TransientFailure)
					channel.ConnectAsync();
				return channel;
			}
		}
		
		public bool CanConnect => !String.IsNullOrEmpty(this.sessionInfoProvider.SessionId);
		
		public virtual void Dispose()
		{
			channel?.ShutdownAsync().Wait();
		}
	}
}
