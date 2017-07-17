using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSTelemetry
{
    [ServiceContract]
    public interface ITelemetryService
    {
		[OperationContract]
		[WebInvoke]
        long SubmitStatistics(TelemetryStatistic statistic);
    }
}
