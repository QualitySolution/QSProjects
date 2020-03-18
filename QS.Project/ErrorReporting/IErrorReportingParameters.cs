namespace QS.ErrorReporting
{
	public interface IErrorReportingParameters
	{
		/// <summary>
		/// Ограничение на кол-во строчек лога в сообщении об ошибке
		/// </summary>
		int? LogRowCount { get; set; }

		/// <summary>
		/// Разрешена автоматическая отправка ошибок
		/// </summary>
		bool CanSendAutomatically { get; set; }

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
