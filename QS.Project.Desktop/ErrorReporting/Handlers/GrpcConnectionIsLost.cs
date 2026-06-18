using System;
using Grpc.Core;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {
	public class GrpcConnectionIsLost : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IErrorReporter errorReporter;
		private readonly IErrorReportingSettings settings;

		public GrpcConnectionIsLost(IInteractiveMessage interactiveMessage, IErrorReporter errorReporter = null, IErrorReportingSettings settings = null) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.errorReporter = errorReporter;
			this.settings = settings;
		}

		public bool Take(Exception exception) {
			var rpcException = exception.FindExceptionTypeInInner<RpcException>();
			if(rpcException == null || !IsConnectionFailure(rpcException))
				return false;

			interactiveMessage.ShowMessage(
				ImportanceLevel.Error,
				"Не удалось связаться с облачным сервисом. Убедитесь что сеть или интернет работают и повторите попытку. " +
				"Если соединение восстановить не удасться, обратитесь к вашему системному администратору.",
				"Ошибка сети"
			);

			if(settings?.SendAutomatically ?? false)
				errorReporter?.SendReport(exception, ErrorType.Known);

			return true;
		}

		private static bool IsConnectionFailure(RpcException exception) {
			switch(exception.StatusCode) {
				case StatusCode.Unavailable:
				case StatusCode.DeadlineExceeded:
					return true;
				case StatusCode.Unknown:
				case StatusCode.Internal:
					return IsConnectionFailureMessage(exception.Status.Detail)
						|| IsConnectionFailureMessage(exception.Message)
						|| IsConnectionFailureMessage(exception.ToString());
				default:
					return false;
			}
		}

		private static bool IsConnectionFailureMessage(string message) {
			if(String.IsNullOrWhiteSpace(message))
				return false;

			return message.IndexOf("Stream removed", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("grpc_message\":\"Stream removed", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("Error received from peer", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("HTTP/2 connection faulted", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("request stream was aborted", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("failed to connect", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("Name resolution failure", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("connection refused", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| message.IndexOf("connection reset", StringComparison.InvariantCultureIgnoreCase) >= 0;
		}
	}
}
