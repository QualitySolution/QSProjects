using System;
using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.ErrorReporting
{
	public class ErrorInfo
	{
		public ErrorInfo(Exception exception = null, ErrorReportType errorReportType = ErrorReportType.Automatic, string email = null, string description = null, UserBase user = null)
		{
			Exceptions = new List<Exception>();
			if(exception != null)
				Exceptions.Add(exception);

			ErrorReportType = errorReportType;
			Email = email;
			Description = description;
			User = User;
		}

		public IList<Exception> Exceptions { get; set; }
		public ErrorReportType ErrorReportType { get; set; }
		public UserBase User { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
	}
}
