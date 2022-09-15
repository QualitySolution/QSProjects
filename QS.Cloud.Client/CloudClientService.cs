using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Grpc.Core.Utils;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
    public class CloudClientService: IDisposable
    {
        public static string ServiceAddress = "core.cloud.qsolution.ru";
        public static int ServicePort = 4200;
        
        private readonly string sessionId;

        private readonly Metadata headers;

        public bool CanConnect => !String.IsNullOrEmpty(sessionId);
        
        public CloudClientService(string sessionId)
        {
            this.sessionId = sessionId;
            headers = new Metadata {{"Authorization", $"Bearer {sessionId}"}};
        }
        
        private Channel channel;
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
        
        public IList<Feature> GetAvailableFeatures(string baseGuid)
        {
            if (string.IsNullOrWhiteSpace(baseGuid))
                throw new ArgumentException("Guid должен быть указан", nameof(baseGuid));

            var client = new CloudFeatures.CloudFeaturesClient(Channel);
            var request = new FeaturesRequest
            {
                BaseGuid = baseGuid
            };
            var response = client.AvailableFeatures(request, headers);

            return response.Features.ToList();
        }
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}
