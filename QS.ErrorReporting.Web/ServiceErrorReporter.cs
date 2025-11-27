using System;
using System.Threading.Tasks;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting
{
	/// <summary>
	/// Базовый класс для отправки отчетов об ошибках
	/// </summary>
	public class ServiceErrorReporter : IServiceErrorReporter
	{
		protected readonly IApplicationInfo application;
		protected readonly IDataBaseInfo databaseInfo;
		protected readonly IAsyncLogService logService;
		protected readonly uint? logRowCount;

		public ServiceErrorReporter(
			IApplicationInfo application, 
			IDataBaseInfo databaseInfo = null, 
			IAsyncLogService logService = null, 
			uint? logRowCount = 300)
		{
			this.application = application ?? throw new ArgumentNullException(nameof(application));
			this.databaseInfo = databaseInfo;
			this.logService = logService;
			this.logRowCount = logRowCount;
		}

		/// <summary>
		/// Отправить отчет об ошибке
		/// </summary>
		public async Task<bool> SendReportAsync(
			Exception exception, 
			ErrorType errorType, 
			string userName = null, 
			string userEmail = null, 
			string databaseName = null)
		{
			using (var reportingService = new ErrorReportingService())
			{
				var log = string.Empty;
				if (logService != null)
					log = await logService.GetLogAsync(logRowCount);

				return reportingService.SubmitErrorReport(
					new SubmitErrorRequest
					{
						App = new AppInfo
						{
							ProductCode = application.ProductCode,
							Modification = application.Modification ?? string.Empty,
							Version = application.Version.ToString(),
						},
						Db = new DatabaseInfo
						{
							Name = databaseName ?? databaseInfo?.Name ?? string.Empty,
						},
						User = new UserInfo
						{
							Email = userEmail ?? string.Empty,
							Name = userName ?? string.Empty,
						},
						Report = new ErrorInfo
						{
							StackTrace = exception.ToString(),
							Log = log,
							Message = exception.Message
						},
						ReportType = (ReportType)(int)errorType
					});
			}
		}
	}
}

