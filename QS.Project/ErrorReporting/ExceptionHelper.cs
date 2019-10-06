using System;
namespace QS.ErrorReporting
{
	public static class ExceptionHelper
	{
		public static TException FineExceptionTypeInInner<TException> (Exception exception)
			where TException : Exception
		{
			if(exception is TException)
				return (TException)exception;

			if(exception.InnerException != null)
				return FineExceptionTypeInInner<TException>(exception.InnerException);
			else
				return null;
		}
	}
}
