using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSBugReporting
{
	[ServiceContract]
	[Obsolete("Здесь интерфейс отправки ошибок просто продублировани, на время доживания QSSupport, пока она не будет совсем удалена.")]
	public interface IBugReportingService
	{
		[OperationContract]
		[WebInvoke]
		bool SubmitBugReport (BugMessage bug);
	}
}