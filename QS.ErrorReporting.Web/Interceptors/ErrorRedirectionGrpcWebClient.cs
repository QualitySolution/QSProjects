using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace QS.ErrorReporting.Web.Interceptors
{
    public class ErrorRedirectionGrpcWebClient: Interceptor
    {
        private readonly ILogger<ErrorRedirectionGrpcWebClient> logger;
        private readonly IGrpcServiceErrorReporter reporter;

        public ErrorRedirectionGrpcWebClient(
            ILogger<ErrorRedirectionGrpcWebClient> logger, 
            IGrpcServiceErrorReporter reporter) 
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try 
            {
                return await continuation(request, context);
            }
            catch (Exception ex) 
            {
                logger.LogError(ex,"Поймана ошибка");
                var result = await reporter.SendReportAsync(ex, context, (ex is RpcException) ? ErrorType.Known : ErrorType.Automatic);
                if(!result)
                    logger.LogError("Ошибка при отправке отчета об ошибке");
                throw;
            }
        }
    }
}
