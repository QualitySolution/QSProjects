using System;
using System.Threading.Tasks;

namespace QS.ErrorReporting {
	public interface IRestServiceErrorReporter {
		Task<bool> SendReportAsync(Exception exception, ErrorType type = ErrorType.Automatic, string databaseName = null);
	}
}
