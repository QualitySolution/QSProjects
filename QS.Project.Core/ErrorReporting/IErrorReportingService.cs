using System.ServiceModel;
using System.ServiceModel.Web;

namespace QS.ErrorReporting
{
	[ServiceContract]
	public interface IErrorReportingService
	{
		[OperationContract]
		[WebInvoke]
		bool SubmitErrorReport (ErrorReport report);
	}
}