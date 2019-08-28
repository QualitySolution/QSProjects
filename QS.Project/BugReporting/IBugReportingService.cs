using System.ServiceModel;
using System.ServiceModel.Web;

namespace QS.BugReporting
{
	[ServiceContract]
	public interface IBugReportingService
	{
		[OperationContract]
		[WebInvoke]
		bool SubmitBugReport (BugMessage bug);
	}
}