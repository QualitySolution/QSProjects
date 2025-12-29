using System;
using System.Threading.Tasks;

namespace QS.ErrorReporting {
	/// <summary>
	/// Сервис отправки отчетов об ошибках для REST сервисов
	/// </summary>
	public interface IRestServiceErrorReporter : IServiceErrorReporter {
		/// <summary>
		/// Отправить отчет об ошибке с указанием имени базы данных
		/// </summary>
		Task<bool> SendReportAsync(Exception exception, ErrorType type = ErrorType.Automatic, string databaseName = null);
	}
}
