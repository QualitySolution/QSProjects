using System;
using System.Threading.Tasks;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	public class RestServiceErrorReporter : IRestServiceErrorReporter {
		private readonly IApplicationInfo application;
		private readonly IDataBaseInfo databaseInfo;
		private readonly IAsyncLogService logService;
		private readonly uint? logRowCount;

		public RestServiceErrorReporter(IApplicationInfo application, IDataBaseInfo databaseInfo = null, IAsyncLogService logService = null, uint? logRowCount = 300) {
			this.application = application ?? throw new ArgumentNullException(nameof(application));
			this.databaseInfo = databaseInfo;
			this.logService = logService;
			this.logRowCount = logRowCount;
		}

		public async Task<bool> SendReportAsync(Exception exception, ErrorType errorType = ErrorType.Automatic, string databaseName = null) {
			using (var reportingService = new ErrorReportingService()) {
				var log = String.Empty;
				if(logService != null)
					log = await logService.GetLogAsync(logRowCount);
				
				return reportingService.SubmitErrorReport(
					new SubmitErrorRequest {
						App = new AppInfo{ 
							ProductCode = application.ProductCode,
							Modification= application.Modification ?? String.Empty,
							Version = application.Version.ToString(),
						},
						Db = new DatabaseInfo {
							Name = databaseName ?? databaseInfo?.Name ?? String.Empty,
						},
						User = new UserInfo {
							Email =  String.Empty,
							Name =  String.Empty,
						},
						Report = new ErrorInfo {
							StackTrace = exception.ToString(),
							Log = log,
							Message = exception.Message
						},
						ReportType =  (ReportType)(int)errorType
					});
			}
		}
	}
}
