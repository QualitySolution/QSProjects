using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSUpdater
{
	[ServiceContract]
	public interface IUpdateService
	{
		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		UpdateResult checkUpdate (string productName, string productEdition, string serial, long major, long minor, long build, long revision);
	}
}

