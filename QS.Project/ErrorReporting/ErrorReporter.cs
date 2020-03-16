using System;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public class ErrorReporter : IErrorReporter
	{
		public ErrorReporter(IErrorReportingService sendService, IApplicationInfo applicationInfo, IDataBaseInfo dataBaseInfo = null, ILogService logService = null)
		{
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.sendService = sendService ?? throw new ArgumentNullException(nameof(sendService));
			this.logService = logService;
			DatabaseInfo = dataBaseInfo;
		}

		IErrorReportingService sendService;
		ILogService logService;

		public IDataBaseInfo DatabaseInfo { get; protected set; }
		public IApplicationInfo ApplicationInfo { get; protected set; }

		public bool SendErrorReport(IErrorReportingSettings settings)
		{
			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseInfo?.Name;
			errorReport.Edition = ApplicationInfo.Edition;
			errorReport.Product = ApplicationInfo.ProductName;
			errorReport.Version = ApplicationInfo.Version.ToString();
			errorReport.Email = settings.Email;
			errorReport.Description = settings.Description;
			errorReport.ReportType = settings.ReportType;
			errorReport.StackTrace = settings.Exception.StackTrace;
			errorReport.UserName = settings.User.Name;
			if(logService != null) {
				errorReport.LogFile = logService.GetLog(settings.LogRowCount);
			}
			if(!settings.CanSendAutomatically || String.IsNullOrWhiteSpace(errorReport.Product) || String.IsNullOrWhiteSpace(errorReport.StackTrace))
				return false;
			return sendService.SubmitErrorReport(errorReport);
		}
	}
}
