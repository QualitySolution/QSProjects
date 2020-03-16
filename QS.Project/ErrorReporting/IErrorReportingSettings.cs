using System;
using QS.Project.Domain;

namespace QS.ErrorReporting
{
	public interface IErrorReportingSettings
	{
		/// <summary>
		/// Ограничение на кол-во строчек лога в сообщении об ошибке
		/// </summary>
		int? LogRowCount { get; set; }

		/// <summary>
		/// Описание для ошибки
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// E-mail отправляющего ошибку 
		/// </summary>
		string Email { get; set; }

		/// <summary>
		/// Тип отправки ошибки
		/// </summary>
		ErrorReportType ReportType { get; set; }

		/// <summary>
		/// Пользователь, отправивший ошибку
		/// </summary>
		UserBase User { get; set; }

		/// <summary>
		/// Сама ошибка
		/// </summary>
		Exception Exception { get; set; }

		/// <summary>
		/// Разрешена автоматическая отправка ошибок
		/// </summary>
		bool CanSendAutomatically { get; set; }
	}
}
