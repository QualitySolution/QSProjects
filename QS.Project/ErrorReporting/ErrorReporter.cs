using System;
using System.Linq;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public class ErrorReporter : IErrorReporter
	{
		public ErrorReporter(IErrorReportingService sendService, IApplicationInfo applicationInfo, IDataBaseInfo dataBaseInfo = null, ILogService logService = null)
		{
			if(applicationInfo == null)
				throw new ArgumentNullException(nameof(applicationInfo));
			this.sendService = sendService ?? throw new ArgumentNullException(nameof(sendService));
			this.logService = logService;
			ProductName = applicationInfo.ProductName;
			Version = applicationInfo.Version.ToString();
			Edition = applicationInfo.Edition;
			DatabaseName = dataBaseInfo.Name;
		}

		IErrorReportingService sendService;
		ILogService logService;

		public string DatabaseName { get; }
		public string ProductName { get; }
		public string Version { get; }
		public string Edition { get; }

		public bool SendErrorReport(IErrorReportingParameters parameters)
		{
			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseName;
			errorReport.Edition = Edition;
			errorReport.Product = ProductName;
			errorReport.Version = Version;
			errorReport.Email = parameters.Email;
			errorReport.Description = parameters.Description;
			errorReport.ReportType = parameters.ReportType;
			errorReport.StackTrace = String.Join("\n Следующее исключение:\n", parameters.Exceptions.Select(x => x.ToString()));
			errorReport.UserName = parameters.User?.Name;
			if(logService != null) {
				errorReport.LogFile = logService.GetLog(parameters.LogRowCount);
			}
			if(!parameters.CanSendAutomatically || String.IsNullOrWhiteSpace(errorReport.Product) || String.IsNullOrWhiteSpace(errorReport.StackTrace))
				return false;
			return sendService.SubmitErrorReport(errorReport);
		}
	}
}
