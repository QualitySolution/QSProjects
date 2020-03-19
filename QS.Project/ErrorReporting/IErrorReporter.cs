namespace QS.ErrorReporting
{
	public interface IErrorReporter
	{
		string DatabaseName { get; }
		string ProductName { get; }
		string Version { get; }
		string Edition { get; }
		bool CanSendAutomatically { get; }

		bool SendErrorReport(ErrorInfo errorInfo);
		bool SendErrorReport(ErrorInfo errorInfo, int logRowCount);
	}
}
