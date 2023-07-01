using System;

namespace QS.ErrorReporting {
	
	/// <summary>
	///Специальный обработчики исключений.
	/// </summary>
	public interface IErrorHandler {
		
		/// <summary>
		/// Метод должен возвращать true, если ошибку он обработал сам 
		/// и ее больше не надо передавать вниз по списку зарегистрированных обработчиков,
		/// вплоть до стандартного диалога отправки отчета об ошибке.
		/// </summary>
		bool Take(Exception exception);
	}
}
