using System;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	public class ErrorReporter : IErrorReporter {
		private readonly IApplicationInfo application;
		private readonly IDataBaseInfo? databaseInfo;
		private readonly IUserInfo? user;
		private readonly ILogService? logService;
		private readonly IErrorReportingSettings? settings;

		public ErrorReporter(IApplicationInfo application, IDataBaseInfo? databaseInfo = null, IUserInfo? user = null, ILogService? logService = null, IErrorReportingSettings? settings = null) {
			this.application = application ?? throw new ArgumentNullException(nameof(application));
			this.databaseInfo = databaseInfo;
			this.user = user;
			this.logService = logService;
			this.settings = settings;
		}

		public bool SendReport(Exception exception, ErrorType errorType = ErrorType.Automatic) {
			using (var reportingService = new ErrorReportingService())
			{
				return reportingService.SubmitErrorReport(
					new SubmitErrorRequest {
						App = new AppInfo{ 
							ProductCode = application.ProductCode,
							ProductName = application.ProductName ?? String.Empty,
							Modification= application.Modification ?? String.Empty,
							Version = application.Version.ToString(),
						},
						Db = new DatabaseInfo {
							Name = databaseInfo?.Name ?? String.Empty,
						},
						User = new UserInfo {
							Email = user?.Email ?? String.Empty,
							Name = user?.Name ?? String.Empty,
						},
						Report = new ErrorInfo {
							StackTrace = exception.ToString(),
							Log = logService?.GetLog(settings?.LogRowCount),
						},
						ReportType =  (ReportType)(int)errorType
					});
			}
		}
	}
}
