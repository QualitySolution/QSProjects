using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSSupportLib
{
	[ServiceContract]
	public interface IUpdateService
	{
		[OperationContract]
		//Аннотация для доступа из браузера и ответа в JSON
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		UpdateResult CheckUpd (string productName, string productEdition, string serial, long major, long minor, long build, long revision);
	}
}

