namespace QS.ErrorReporting
{
	public interface IErrorReportSender
	{
		bool SubmitErrorReport (ErrorReport report);
	}
}
