using System;
using System.Collections.Generic;

namespace QS.Utilities.Debug {
	public static class ExceptionHelper {
		/// <summary>
		/// Метод ищет TException тип исключения среди вложенных исключений.
		/// </summary>
		/// <param name="exception">Корневое исключений</param>
		/// <typeparam name="TException">Тип исключения который необходимо найти</typeparam>
		/// <returns></returns>
		public static TException FindExceptionTypeInInner<TException>(this Exception exception)
			where TException : Exception {
			if(exception is TException)
				return (TException)exception;

			if(exception.InnerException != null)
				return exception.InnerException.FindExceptionTypeInInner<TException>();
			else
				return null;
		}

		/// <summary>
		/// Метод возвращает все исключения типа TException среди вложенных.
		/// </summary>
		/// <param name="exception">Корневое исключений</param>
		/// <typeparam name="TException">Тип исключения который необходимо найти</typeparam>
		/// <returns></returns>
		public static IEnumerable<TException> FindAllExceptionTypeInInner<TException>(this Exception exception)
			where TException : Exception {
			if(exception is TException)
				yield return (TException)exception;

			if(exception.InnerException != null)
				foreach(var inner in exception.InnerException.FindAllExceptionTypeInInner<TException>())
					yield return inner;
		}
	}
}
