using System.ServiceModel;
using System.ServiceModel.Web;
using QSUpdater.DTO;

namespace QSUpdater
{
    [ServiceContract]
    public interface IUpdateService
    {
        [OperationContract]// (IsInitiating = true, IsTerminating = true)] 
                           //FIXME на Windows при наличии этих атрибутов стали получать Эксепшен. Отключил до дальнейших разбирательств.
        UpdateResult checkForUpdate(string parameters);

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        UsingStatisticsResult GetUsingStatistics(string product, int days);
    }
}