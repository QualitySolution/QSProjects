namespace QS.ErrorReporting
{
	public interface IErrorDialogSettings
	{
		/// <summary>
		/// Требовать почту для отправки сообщения
		/// </summary>
		bool RequestEmail { get; }

		/// <summary>
		/// Требовать описания ошибки для отправки сообщения
		/// </summary>
		bool RequestDescription { get; }
	}
}
