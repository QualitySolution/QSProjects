using System;
using System.Collections.Generic;

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

		public static IList<TException> FindAllExceptionsWithTypeInInner<TException>(this Exception exception)
			where TException : Exception
		{
			var exceptions = new List<TException>();

			while(exception != null)
			{
				if(exception is AggregateException aggregateException)
				{
					exceptions.AddRange(aggregateException.FindAllExceptionsWithTypeInInner<TException>());
				}
				if(exception is TException tException)
				{
					exceptions.Add(tException);
				}

				exception = exception.InnerException;
			}

			return exceptions;
		}
	}
}
