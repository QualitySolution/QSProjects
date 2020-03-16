using System;
using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.ErrorReporting
{
	public class ErrorReportingParameters : IErrorReportingParameters
	{
		public ErrorReportingParameters(
			int? logRowCount,
			bool canSendAutomatically,
			UserBase user,
			string email,
			string description,
			ErrorReportType reportType
		)
		{
			LogRowCount = logRowCount;
			Description = description;
			Email = email;
			ReportType = reportType;
			User = user;
			Exceptions = new List<Exception>();
			CanSendAutomatically = canSendAutomatically;
		}

		public int? LogRowCount { get; set; }
		public string Description { get; set; }
		public string Email { get; set; }
		public ErrorReportType ReportType { get; set; }
		public UserBase User { get; set; }
		public IList<Exception> Exceptions { get; set; }
		public bool CanSendAutomatically { get; set; }
	}
}
