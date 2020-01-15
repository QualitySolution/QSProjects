using System;
using System.Runtime.Serialization;

namespace QS.Views.Resolve
{
	public class GtkViewResolveException : Exception
	{
		public GtkViewResolveException()
		{
		}

		public GtkViewResolveException(string message) : base(message)
		{
		}

		public GtkViewResolveException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected GtkViewResolveException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
