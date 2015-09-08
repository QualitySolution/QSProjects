using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSBugReporting
{
	[ServiceContract]
	public interface IBugReportingService
	{
		[OperationContract]
		[WebInvoke (BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Xml,
			Method = "GET"
		)]
		bool SubmitBugReport (string product, string version, string stackTrace, string description, string email, string userName, string logFile);
	}
}