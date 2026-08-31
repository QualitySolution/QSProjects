using System;

namespace QS.ErrorReporting {
	public class OperationRefusedException : Exception {
		public OperationRefusedException() { }

		public OperationRefusedException(string message) : base(message) { }

		public OperationRefusedException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
