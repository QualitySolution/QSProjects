using System;

namespace QS.ErrorReporting {
	
	/// <summary>
	/// Сервис отправки куда надо сообщений об ошибка
	/// </summary>
	public interface IErrorReporter {
		
		/// <summary>
		/// Отправить сообщение.
		/// Все параметры отправки кроме исключения являются не обязательными.
		/// Не стесняйтесь их дополнять при необходимости. Изначально реализован самый минимум.
		/// </summary>
		/// <param name="exception">Исключение</param>
		/// <returns>true если сообщение отправлено</returns>
		bool SendReport(Exception exception, ErrorType type = ErrorType.Automatic);
	}

	//Внимание! Добавляя значение в этот ENUM убедитесь что оно так же добавлено в ReportType из QS.ErrorReporting.Client,
	//так как класс DesktopErrorReporter рассчитывает что значения по int будут идентичны в обоих перечислениях.
	public enum ErrorType {
		User = 0,
		Automatic,
		Known
	}
}
