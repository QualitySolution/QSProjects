using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
    public class CloudFeaturesClient: CloudClientServiceBase
    {
        public CloudFeaturesClient(ISessionInfoProvider sessionInfoProvider) 
	        : base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }
        
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
    }
}
