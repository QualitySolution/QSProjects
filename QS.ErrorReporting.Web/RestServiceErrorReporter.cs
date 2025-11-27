using System;
using System.Threading.Tasks;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	public class RestServiceErrorReporter : ServiceErrorReporter, IRestServiceErrorReporter {
		
		public RestServiceErrorReporter(
			IApplicationInfo application, 
			IDataBaseInfo databaseInfo = null, 
			IAsyncLogService logService = null, 
			uint? logRowCount = 300) 
			: base(application, databaseInfo, logService, logRowCount)
		{
		}

		/// <summary>
		/// Отправить отчет об ошибке с указанием имени базы данных
		/// </summary>
		public Task<bool> SendReportAsync(Exception exception, ErrorType errorType = ErrorType.Automatic, string databaseName = null)
		{
			return SendReportAsync(exception, errorType, null, null, databaseName);
		}
	}
}
