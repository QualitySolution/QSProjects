using System;
using System.Linq;
using Grpc.Core;

namespace QS.Updates
{
    public class ReleasesService: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
        
        public static string ServiceAddress = "updates.cloud.qsolution.ru";
        public static int ServicePort = 4203;

        public ReleasesService()
        {
        }
        
        private Channel? channel;
        private Channel Channel {
            get {
                if(channel == null || channel.State == ChannelState.Shutdown)
                    channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
                if (channel.State == ChannelState.TransientFailure)
                    channel.ConnectAsync();
                return channel;
            }
        }
        
        #region Запросы
        
        public CheckForUpdatesResponse CheckForUpdates(int productCode, string version, string modification = "", string serial = "", ReleaseChannel releaseChannel = ReleaseChannel.Current)
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
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}
