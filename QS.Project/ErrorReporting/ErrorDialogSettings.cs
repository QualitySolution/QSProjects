namespace QS.ErrorReporting
{
	public class ErrorDialogSettings : IErrorDialogSettings
	{
		public bool RequestEmail => false;
		public bool RequestDescription => true;
	}
}
