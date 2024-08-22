using System;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public abstract class CloudClientServiceBase : IDisposable
	{
		private readonly string serviceAddress;
		private readonly int servicePort;

		protected Metadata headers;
        
		public CloudClientServiceBase(string serviceAddress, int servicePort)
		{
			this.serviceAddress = serviceAddress;
			this.servicePort = servicePort;
		}

		private Channel channel;
		protected Channel Channel
		{
			get
			{
				if(channel == null || channel.State == ChannelState.Shutdown)
					channel = new Channel(serviceAddress, servicePort, ChannelCredentials.Insecure);
				if (channel.State == ChannelState.TransientFailure)
					channel.ConnectAsync();
				return channel;
			}
		}
		
		public abstract bool CanConnect { get; }
		
		public virtual void Dispose()
		{
			channel?.ShutdownAsync().Wait();
		}
	}
}
