using System;
using System.Linq;
using QS.ErrorReporting;
using QS.Project.VersionControl;

namespace Vodovoz.Tools
{
	public class ErrorReporter : IErrorReporter
	{
		public ErrorReporter(
			IErrorReportingService sendService,
			IApplicationInfo applicationInfo,
			ILogService logService = null,
			IDataBaseInfo dataBaseInfo = null
		)
		{
			this.sendService = sendService ?? throw new ArgumentNullException(nameof(sendService));
			if(applicationInfo == null)
				throw new ArgumentNullException(nameof(applicationInfo));
			Edition = applicationInfo.Edition;
			Version = applicationInfo.Version.ToString();
			ProductName = applicationInfo.ProductName;
			DatabaseName = dataBaseInfo?.Name;
			this.logService = logService;
			autoSendLogRowCount = 300;
		}

		ILogService logService;
		int? autoSendLogRowCount;
		IErrorReportingService sendService;

		public string DatabaseName { get; protected set; }
		public string ProductName { get; protected set; }
		public string Version { get; protected set; }
		public string Edition { get; protected set; }

		public bool CanSendAutomatically => true;

		public bool SendErrorReport(ErrorInfo errorInfo)
		{
			if(!Validate(errorInfo))
				return false;

			var errorReport = PrepareReportInfo(errorInfo);
			return sendService.SubmitErrorReport(errorReport);
		}

		public bool SendErrorReport(ErrorInfo errorInfo, int logRowsCount)
		{
			if(!Validate(errorInfo))
				return false;

			var errorReport = PrepareReportInfo(errorInfo, logRowsCount);
			return sendService.SubmitErrorReport(errorReport);
		}

		#region Helpers

		private bool Validate(ErrorInfo errorInfo)
		{
			if(errorInfo.ErrorReportType == ErrorReportType.Automatic && !CanSendAutomatically)
				return false;

			return true;
		}

		private ErrorReport PrepareReportInfo(ErrorInfo errorInfo, int? logRowOverrideCount = null)
		{
			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseName;
			errorReport.Edition = Edition;
			errorReport.Product = ProductName;
			errorReport.Version = Version;
			errorReport.Email = errorInfo.Email;
			errorReport.Description = errorInfo.Description;
			errorReport.ReportType = errorInfo.ErrorReportType;
			errorReport.StackTrace = GetExceptionText(errorInfo);
			errorReport.UserName = errorInfo.User?.Name;

			if(logService != null) {
				if(logRowOverrideCount != null)
					errorReport.LogFile = logService.GetLog(logRowOverrideCount);
				else {
					if(errorInfo.ErrorReportType == ErrorReportType.Automatic)
						errorReport.LogFile = logService.GetLog(autoSendLogRowCount);
					else
						errorReport.LogFile = logService.GetLog();
				}
			}
			return errorReport;
		}

		public string GetExceptionText(ErrorInfo errorInfo) =>
			string.Join("\n Следующее исключение:\n", errorInfo?.Exceptions?.Select(ex => ex.ToString()));

		#endregion
	}
}
