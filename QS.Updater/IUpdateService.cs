using System.ServiceModel;
using System.ServiceModel.Web;
using QS.Updater.DTO;
using QSUpdater;

namespace QS.Updater
{
    [ServiceContract]
    public interface IUpdateService
    {
        [OperationContract]
        UpdateResult checkForUpdate(string parameters);

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        UsingStatisticsResult GetUsingStatistics(string product, int days, int minCount = 0);
    }
}