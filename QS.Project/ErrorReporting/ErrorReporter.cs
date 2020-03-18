using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public class ErrorReporter : PropertyChangedBase, IErrorReporter
	{
		bool reportSent;

		private readonly IErrorReportingParameters reportingParameters;
		private readonly Func<IErrorReportingService> sendServiceFactory;
		private readonly IApplicationInfo applicationInfo;
		private readonly IInteractiveMessage interactive;
		ILogService logService;

		public ErrorReporter(Func<IErrorReportingService> sendServiceFactory, IErrorReportingParameters reportingParameters, IApplicationInfo applicationInfo, IInteractiveMessage interactive, IDataBaseInfo dataBaseInfo = null, UserBase user = null, ILogService logService = null)
		{
			this.sendServiceFactory = sendServiceFactory ?? throw new ArgumentNullException(nameof(sendServiceFactory));
			this.reportingParameters = reportingParameters ?? throw new ArgumentNullException(nameof(reportingParameters));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			DataBaseInfo = dataBaseInfo;
			User = user;
			this.logService = logService;
		}

		public string DatabaseName => DataBaseInfo?.Name;
		public string ProductName => applicationInfo.ProductName;
		public string Version => applicationInfo.Version?.ToString();
		public string Edition => applicationInfo.Edition;

		private IList<Exception> Exceptions = new List<Exception>();

		public string ExceptionText =>
			string.Join("\n Следующее исключение:\n", Exceptions.Select(ex => ex.ToString()));

		public string Description { get; set ; }
		public string Email { get; set; }

		public bool CanSendReport => (!reportingParameters.RequestDescription || !String.IsNullOrWhiteSpace(Description))
				&& (!reportingParameters.RequestEmail || EmailIsValid);

		public bool EmailIsValid => new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$").IsMatch(Email);

		public IDataBaseInfo DataBaseInfo { get; set; }
		public UserBase User { get; set; }

		public bool SendErrorReport(ErrorReportType reportType)
		{
			if (reportType == ErrorReportType.Automatic && !reportingParameters.CanSendAutomatically || reportSent)
				return false;

			if (reportType == ErrorReportType.User && !CanSendReport)
				return false;

			if (String.IsNullOrWhiteSpace(ProductName) || String.IsNullOrWhiteSpace(ExceptionText))
				return false;

			var sendService = sendServiceFactory();
			if (sendService == null) {
				interactive.ShowMessage(ImportanceLevel.Error, "Не удалось установить соединение с сервером Quality Solution.");
				return false;
			}

			ErrorReport errorReport = new ErrorReport();
			errorReport.DBName = DatabaseName;
			errorReport.Edition = Edition;
			errorReport.Product = ProductName;
			errorReport.Version = Version;
			errorReport.Email = Email;
			errorReport.Description = Description;
			errorReport.ReportType = reportType;
			errorReport.StackTrace = ExceptionText;
			errorReport.UserName = User?.Name;
			if(logService != null) {
				errorReport.LogFile = logService.GetLog(reportType == ErrorReportType.Automatic ? reportingParameters.LogRowCount : null);
			}

			reportSent = sendService.SubmitErrorReport(errorReport);
			return reportSent;
		}

		public void AddException(Exception exception)
		{
			Exceptions.Add(exception);
			OnPropertyChanged(nameof(ExceptionText));
		}
	}
}