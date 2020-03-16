using System;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public class SingletonErrorReporter : IErrorReporter
	{
		IErrorReportingService sendService;
		ILogService logService;

		public IDataBaseInfo DatabaseInfo { get; protected set; }
		public IApplicationInfo ApplicationInfo { get; protected set; }

		static SingletonErrorReporter instance;
		public static SingletonErrorReporter Instance {
			get {
				if(instance == null)
					instance = new SingletonErrorReporter();
				return instance;
			}
		}

		protected SingletonErrorReporter() { }

		public static void Initialize(IErrorReportingService sendService, IApplicationInfo applicationInfo, IDataBaseInfo dataBaseInfo = null, ILogService logService = null)
		{
			instance = new SingletonErrorReporter();
			Instance.sendService = sendService ?? throw new ArgumentNullException(nameof(sendService));
			Instance.ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			Instance.logService = logService;
			Instance.DatabaseInfo = dataBaseInfo;
		}

		public bool SendErrorReport(IErrorReportingSettings settings)
		{
			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseInfo.Name;
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
