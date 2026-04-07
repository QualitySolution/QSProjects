using System;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public abstract class CloudClientServiceBase : IDisposable
	{
		private readonly string serviceAddress;
		private readonly int servicePort;
		private readonly ChannelCredentials credentials;

		protected Metadata headers;
        
		public CloudClientServiceBase(string serviceAddress, int servicePort, ChannelCredentials credentials = null)
		{
			this.serviceAddress = serviceAddress;
			this.servicePort = servicePort;
			this.credentials = credentials ?? (servicePort == 443 ? (ChannelCredentials)new SslCredentials() : ChannelCredentials.Insecure);
		}

		private Channel channel;
		protected Channel Channel
		{
			get
			{
				if(channel == null || channel.State == ChannelState.Shutdown) {
					var channelOptions = new[]
					{
						new ChannelOption(ChannelOptions.MaxReceiveMessageLength, 10 * 1024 * 1024), // 10MB
						new ChannelOption(ChannelOptions.MaxSendMessageLength, 10 * 1024 * 1024) // 10MB
					};
					channel = new Channel(serviceAddress, servicePort, credentials, channelOptions);
				}
				if (channel.State == ChannelState.TransientFailure)
					channel.ConnectAsync();
				return channel;
			}
		}
		
		public abstract bool CanConnect { get; }
		
		public virtual async void Dispose()
		{
			await channel?.ShutdownAsync();
		}
	}
}
