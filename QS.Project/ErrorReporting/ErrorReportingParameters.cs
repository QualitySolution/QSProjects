namespace QS.ErrorReporting
{
	public class ErrorReportingParameters : IErrorReportingParameters
	{
		public ErrorReportingParameters(bool requestEmail, bool requestDescription, bool canSendAutomatically, int? logRowCount)
		{
			CanSendAutomatically = canSendAutomatically;
			LogRowCount = logRowCount;
			RequestEmail = requestEmail;
			RequestDescription = requestDescription;
		}

		public bool CanSendAutomatically { get; set; }

		public int? LogRowCount { get; set; }

		public bool RequestEmail { get; set; }

		public bool RequestDescription { get; set; }
	}
}
