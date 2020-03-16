using System;
using QS.Project.Domain;

namespace QS.ErrorReporting
{
	public class ErrorReportingParametersFactory : IErrorReportingParametersFactory
	{
		int? logRowCount;
		bool canSendAutomatically;
		UserBase user;
		ErrorReportType reportType;
		string email;
		string description;

		public ErrorReportingParametersFactory(
			int? logRowCount = null,
			bool canSendAutomatically = true,
			UserBase user = null,
			ErrorReportType reportType = ErrorReportType.Automatic,
			string email = null,
			string description = null
		)
		{
			this.logRowCount = logRowCount;
			this.canSendAutomatically = canSendAutomatically;
			this.user = user;
			this.reportType = reportType;
			this.email = email;
			this.description = description;
		}

		public IErrorReportingParameters CreateParameters()
		{
			return new ErrorReportingParameters(logRowCount, canSendAutomatically, user, email, description, reportType);
		}
	}
}
