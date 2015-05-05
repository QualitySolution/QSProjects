using System.ServiceModel;

namespace QSUpdater
{
	[ServiceContract]
	public interface IUpdateService
	{
		[OperationContract (IsInitiating = true, IsTerminating = true)]
		UpdateResult checkForUpdate (string parameters);
	}
}