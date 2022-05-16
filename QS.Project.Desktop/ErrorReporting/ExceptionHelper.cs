using System;
namespace QS.ErrorReporting
{
	public static class ExceptionHelper
	{
		/// <summary>
		/// Метод ищет TException тип исключения среди вложенных исключений.
		/// </summary>
		/// <param name="exception">Корневое исключений</param>
		/// <typeparam name="TException">Тип исключения который необходимо найти</typeparam>
		/// <returns></returns>
		public static TException FindExceptionTypeInInner<TException> (this Exception exception)
			where TException : Exception
		{
			if(exception is TException)
				return (TException)exception;

			if(exception.InnerException != null)
				return FindExceptionTypeInInner<TException>(exception.InnerException);
			else
				return null;
		}
	}
}
