using System;
namespace QS.ErrorReporting
{
	public interface IErrorReportingSettings
	{
		/// <summary>
		/// Отправлять сообщение об ошибке автоматически
		/// </summary>
		bool SendAutomatically { get; }

		/// <summary>
		/// Ограничение на кол-во строчек лога в сообщении об ошибке(отправленной автоматически)
		/// </summary>
		int? LogRowCount { get; }

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
