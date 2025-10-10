using System;
using System.Threading.Tasks;
using Grpc.Core;
using QS.ErrorReporting.Web.Authentication;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting.Web {
	public class GrpcServiceErrorReporter : IGrpcServiceErrorReporter {
		private readonly IApplicationInfo application;
		private readonly IDataBaseInfo databaseInfo;
		private readonly IAsyncUserInfo user;
		private readonly IAsyncLogService logService;
		private readonly uint? logRowCount;

		public GrpcServiceErrorReporter(IApplicationInfo application, IDataBaseInfo databaseInfo = null, IAsyncUserInfo user = null, IAsyncLogService logService = null, uint? logRowCount = 300) {
			this.application = application ?? throw new ArgumentNullException(nameof(application));
			this.databaseInfo = databaseInfo;
			this.user = user;
			this.logService = logService;
			this.logRowCount = logRowCount;
		}

		public async Task<bool> SendReportAsync(Exception exception, ServerCallContext context, ErrorType errorType = ErrorType.Automatic) {
			using (var reportingService = new ErrorReportingService()) {
				var userName = String.Empty; 
				var userEmail = String.Empty;
				if(user != null) {
					userName = await user.GetNameAsync(context);
				}

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
							Name = databaseInfo?.Name ?? String.Empty,
						},
						User = new UserInfo {
							Email = userName,
							Name =  userEmail,
						},
						Report = new ErrorInfo {
							StackTrace = exception.ToString(),
							Log = log,
						},
						ReportType =  (ReportType)(int)errorType
					});
			}
		}
	}
}
