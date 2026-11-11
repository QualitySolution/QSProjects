using System;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Updates
{
    public class ReleasesService: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
        
        public static string ServiceAddress = "updates.cloud.qsolution.ru";
        public static int ServicePort = 443;

        private static ChannelCredentials Credentials =>
            ServicePort == 443 ? (ChannelCredentials)new SslCredentials() : ChannelCredentials.Insecure;

        public ReleasesService()
        {
        }
        
        private Channel? channel;
        private Channel Channel {
            get {
                if(channel == null || channel.State == ChannelState.Shutdown)
                    channel = new Channel(ServiceAddress, ServicePort, Credentials);
                if (channel.State == ChannelState.TransientFailure)
                    channel.ConnectAsync();
                return channel;
            }
        }
        
        #region Запросы
        
        public virtual CheckForUpdatesResponse CheckForUpdates(int productCode, string version, string modification = "", string serial = "", ReleaseChannel releaseChannel = ReleaseChannel.Current)
        {
            var client = new Releases.ReleasesClient(Channel);
            var request = new CheckForUpdatesRequest() {
                Channel = releaseChannel,
				ProductCode = productCode,
				Version = version,
				Modification = modification,
				SerialNumber = serial
            };
            return client.CheckForUpdates(request);
        }
        
        public virtual async Task<CheckForUpdatesResponse> CheckForUpdatesAsync(int productCode, string version, string modification, string serial, ReleaseChannel releaseChannel, CancellationToken cancellationToken)
        {
            var client = new Releases.ReleasesClient(Channel);
            var request = new CheckForUpdatesRequest {
                Channel = releaseChannel, ProductCode = productCode, Version = version,
                Modification = modification, SerialNumber = serial
            };
            return await client.CheckForUpdatesAsync(request, deadline: DateTime.UtcNow.AddSeconds(15), cancellationToken: cancellationToken).ResponseAsync.ConfigureAwait(false);
        }

        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}
