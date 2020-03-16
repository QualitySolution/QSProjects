namespace QS.ErrorReporting
{
	public interface IErrorReporter
	{
		string DatabaseName { get; }
		string ProductName { get; }
		string Version { get; }
		string Edition { get; }

		bool SendErrorReport(IErrorReportingParameters parameters);
	}
}
