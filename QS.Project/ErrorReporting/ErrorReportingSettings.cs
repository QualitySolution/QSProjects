using System;
namespace QS.ErrorReporting
{
	public class ErrorReportingSettings : IErrorReportingSettings
	{

		public ErrorReportingSettings(bool requestEmail, bool requestDescription, bool sendAutomatically, int? logRowCount)
		{
			SendAutomatically = sendAutomatically;
			LogRowCount = logRowCount;
			RequestEmail = requestEmail;
			RequestDescription = requestDescription;
		}

		public bool SendAutomatically { get; set; }

		public int? LogRowCount { get; set; }

		public bool RequestEmail { get; set; }

		public bool RequestDescription { get; set; }
	}
}
