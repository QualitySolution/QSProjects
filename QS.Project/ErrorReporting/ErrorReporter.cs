using System;
using System.Linq;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public class ErrorReporter : IErrorReporter
	{
		public ErrorReporter(
			IErrorReportingService sendService,
			IApplicationInfo applicationInfo,
			ILogService logService = null,
			IDataBaseInfo dataBaseInfo = null,
			bool canSendAutomatically = true
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
			CanSendAutomatically = canSendAutomatically;
		}

		ILogService logService;
		int? autoSendLogRowCount;
		IErrorReportingService sendService;

		public string DatabaseName { get; protected set; }
		public string ProductName { get; protected set; }
		public string Version { get; protected set; }
		public string Edition { get; protected set; }

		public bool CanSendAutomatically { get; protected set; }

		public bool SendErrorReport(
			Exception[] exceptions,
			ErrorReportType errorReportType,
			string description,
			string email,
			UserBase user
		)
		{
			if(errorReportType == ErrorReportType.Automatic && !CanSendAutomatically)
				return false;

			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseName;
			errorReport.Edition = Edition;
			errorReport.Product = ProductName;
			errorReport.Version = Version;
			errorReport.Email = email;
			errorReport.Description = description;
			errorReport.ReportType = errorReportType;
			errorReport.StackTrace = GetExceptionText(exceptions);
			errorReport.UserName = user?.Name;

			errorReport = PrepareLog(errorReport);
			return sendService.SubmitErrorReport(errorReport);
		}

		public bool SendErrorReport(
			Exception[] exceptions,
			int logRowsCount,
			ErrorReportType errorReportType,
			string description,
			string email,
			UserBase user
		)
		{
			if(errorReportType == ErrorReportType.Automatic && !CanSendAutomatically)
				return false;

			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseName;
			errorReport.Edition = Edition;
			errorReport.Product = ProductName;
			errorReport.Version = Version;
			errorReport.Email = email;
			errorReport.Description = description;
			errorReport.ReportType = errorReportType;
			errorReport.StackTrace = GetExceptionText(exceptions);
			errorReport.UserName = user?.Name;

			errorReport = PrepareLog(errorReport, logRowsCount);
			return sendService.SubmitErrorReport(errorReport);
		}

		private ErrorReport PrepareLog(ErrorReport errorReport, int? logRowOverrideCount = null)
		{
			if(logService != null) {
				if(logRowOverrideCount != null)
					errorReport.LogFile = logService.GetLog(logRowOverrideCount);
				else {
					if(errorReport.ReportType == ErrorReportType.Automatic)
						errorReport.LogFile = logService.GetLog(autoSendLogRowCount);
					else
						errorReport.LogFile = logService.GetLog();
				}
			}
			return errorReport;
		}

		public string GetExceptionText(Exception[] exceptions) =>
			string.Join("\n Следующее исключение:\n", exceptions.Select(ex => ex.ToString()));
	}
}
