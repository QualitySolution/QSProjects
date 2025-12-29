using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QS.ErrorReporting.Middlewares {
	public class ErrorHandlingMiddleware {
		private readonly RequestDelegate _next;
		private readonly IRestServiceErrorReporter _reporter;
		private readonly ILogger<ErrorHandlingMiddleware> _logger;
		public ErrorHandlingMiddleware(RequestDelegate next, IRestServiceErrorReporter reporter, ILogger<ErrorHandlingMiddleware> logger) {
			_next = next;
			_logger = logger;
			_reporter = reporter;
		}

		public async Task Invoke(HttpContext context) {
			try {
				await _next(context);
			}
			catch(Exception ex) {
				_logger.LogError(ex, "Поймана ошибка: {RequestPath}, Headers: {Headers}", context.Request.Path, context.Request.Headers);
				var res = await _reporter.SendReportAsync(ex);
				if(!res)
					_logger.LogError("Ошибка при отправке отчета об ошибке");
				throw; 
			}
		}
	}
}
