using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
    public class CloudClientService: IDisposable
    {
        public static string ServiceAddress = "core.cloud.qsolution.ru";
        public static int ServicePort = 4200;
        
        private readonly Channel channel;
        
        public CloudClientService()
        {
            channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
        }
        
        #region Запросы
        
        public IList<Feature> GetAvailableFeatures(string baseGuid)
        {
            if (string.IsNullOrWhiteSpace(baseGuid))
                throw new ArgumentException("Guid должен быть указан", nameof(baseGuid));
            
            var client = new CloudFeatures.CloudFeaturesClient(channel);
            var request = new FeaturesRequest
            {
                BaseGuid = baseGuid
            };
            var response = client.AvailableFeatures(request);

            return response.Features.ToList();
        }
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}