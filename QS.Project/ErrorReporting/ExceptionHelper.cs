using System;
namespace QS.ErrorReporting
{
	public static class ExceptionHelper
	{
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
