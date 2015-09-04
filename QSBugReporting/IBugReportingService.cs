using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSBugReporting
{
	[ServiceContract]
	public interface IBugReportingService
	{
		[OperationContract]
		[WebInvokeAttribute (BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
		bool SubmitBugReport (string product, string version, string stackTrace, string description, string email, string userName, string logFile);
	}
}

