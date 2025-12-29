using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace QS.ErrorReporting {
	
	/// <summary>
	/// Сервис отправки отчетов об ошибках для gRPC сервисов
	/// </summary>
	public interface IGrpcServiceErrorReporter : IServiceErrorReporter {
	
		/// <summary>
		/// Отправить сообщение с контекстом gRPC.
		/// Все параметры отправки кроме исключения являются не обязательными.
		/// Не стесняйтесь их дополнять при необходимости. Изначально реализован самый минимум.
		/// </summary>
		/// <param name="exception">Исключение</param>
		/// <param name="context">Контекст gRPC вызова</param>
		/// <param name="type">Тип ошибки</param>
		/// <returns>true если сообщение отправлено</returns>
		Task<bool> SendReportAsync(Exception exception, ServerCallContext context, ErrorType type = ErrorType.Automatic);
	}
}
