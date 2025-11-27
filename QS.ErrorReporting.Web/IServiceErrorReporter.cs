using System;
using System.Threading.Tasks;

namespace QS.ErrorReporting
{
	/// <summary>
	/// Базовый интерфейс для отправки отчетов об ошибках
	/// </summary>
	public interface IServiceErrorReporter
	{
		/// <summary>
		/// Отправить отчет об ошибке
		/// </summary>
		/// <param name="exception">Исключение</param>
		/// <param name="type">Тип ошибки</param>
		/// <param name="userName">Имя пользователя, при его наличии</param>
		/// <param name="userEmail">Email пользователя, при его наличии</param>
		/// <param name="databaseName">Имя базы данных, при его наличии</param>
		/// <returns>true если сообщение отправлено</returns>
		Task<bool> SendReportAsync(Exception exception, ErrorType type = ErrorType.Automatic, string userName = null, string userEmail = null, string databaseName = null);
	}
}

