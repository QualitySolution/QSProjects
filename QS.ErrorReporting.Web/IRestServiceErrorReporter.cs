using System;
using System.Threading.Tasks;

namespace QS.ErrorReporting.Web {
	public interface IRestServiceErrorReporter {
		Task<bool> SendReportAsync(Exception exception, ErrorType type = ErrorType.Automatic, string databaseName = null);
	}
}
