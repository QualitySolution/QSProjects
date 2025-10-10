using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace QS.ErrorReporting {
	
	/// <summary>
	/// Сервис отправки куда надо сообщений об ошибка
	/// </summary>
	public interface IGrpcServiceErrorReporter {
	
		/// <summary>
		/// Отправить сообщение.
		/// Все параметры отправки кроме исключения являются не обязательными.
		/// Не стесняйтесь их дополнять при необходимости. Изначально реализован самый минимум.
		/// </summary>
		/// <param name="exception">Исключение</param>
		/// <returns>true если сообщение отправлено</returns>
		Task<bool> SendReportAsync(Exception exception, ServerCallContext context, ErrorType type = ErrorType.Automatic);
	}
}
