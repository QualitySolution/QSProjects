using System.ServiceModel;

namespace QSUpdater
{
	[ServiceContract]
	public interface IUpdateService
	{
		[OperationContract]
		UpdateResult checkForUpdate (string parameters);
	}
}