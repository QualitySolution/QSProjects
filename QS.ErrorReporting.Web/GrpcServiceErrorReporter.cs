using System;
using System.Threading.Tasks;
using Grpc.Core;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	public class GrpcServiceErrorReporter : ServiceErrorReporter, IGrpcServiceErrorReporter {
		private readonly IAsyncUserInfo user;

		public GrpcServiceErrorReporter(
			IApplicationInfo application, 
			IDataBaseInfo databaseInfo = null, 
			IAsyncUserInfo user = null, 
			IAsyncLogService logService = null, 
			uint? logRowCount = 300) 
			: base(application, databaseInfo, logService, logRowCount)
		{
			this.user = user;
		}

		/// <summary>
		/// Отправить отчет об ошибке с контекстом gRPC
		/// </summary>
		public async Task<bool> SendReportAsync(Exception exception, ServerCallContext context, ErrorType errorType = ErrorType.Automatic) {
			var userName = string.Empty; 
			var userEmail = string.Empty;
			if(user != null) {
				userName = await user.GetNameAsync(context);
			}

			return await SendReportAsync(exception, errorType, userName, userEmail);
		}
	}
}
