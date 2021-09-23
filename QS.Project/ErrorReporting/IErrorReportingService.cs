namespace QS.ErrorReporting
{
	public interface IErrorReportingService
	{
		bool SubmitErrorReport (ErrorReport report);
	}
}
