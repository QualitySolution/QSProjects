using System;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public abstract class CloudClientServiceBase : IDisposable
	{
		private readonly string serviceAddress;
		private readonly int servicePort;
		private readonly ChannelCredentials credentials;

		//Локальная отладка: при заданном OverrideAddress лаунчер подключается к локально
		//поднятому серверу (localhost:4200) без SSL, минуя облачный адрес.
		public static string OverrideAddress { get; set; } = "localhost";
		public static int? OverridePort { get; set; } = 4200;
		public static bool UseInsecureOverride { get; set; } = true;

		protected Metadata headers;

		public CloudClientServiceBase(string serviceAddress, int servicePort, ChannelCredentials credentials = null)
		{
			this.serviceAddress = OverrideAddress ?? serviceAddress;
			this.servicePort = OverridePort ?? servicePort;
			var useInsecure = OverrideAddress != null && UseInsecureOverride;
			this.credentials = credentials ?? (useInsecure
				? ChannelCredentials.Insecure
				: (this.servicePort == 443 ? (ChannelCredentials)new SslCredentials() : ChannelCredentials.Insecure));
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
