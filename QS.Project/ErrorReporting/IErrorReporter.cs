using System;
using System.ComponentModel;

namespace QS.ErrorReporting
{
	public interface IErrorReporter : INotifyPropertyChanged
	{
		#region Только чтение
		string DatabaseName { get; }
		string ProductName { get; }
		string Version { get; }
		string Edition { get; }

		string ExceptionText { get; }
		bool CanSendReport { get; }
		bool EmailIsValid { get; }
		#endregion

		#region Установка параметров

		/// <summary>
		/// Описание для ошибки
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// E-mail отправляющего ошибку 
		/// </summary>
		string Email { get; set; }

		#endregion

		void AddException(Exception exception);

		bool SendErrorReport(ErrorReportType reportType);
	}
}
