using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QS.ErrorReporting
{
    /// <summary>
    /// Базовый класс для фоновых сервисов с автоматической отправкой ошибок
    /// </summary>
    public abstract class ErrorReportingBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceErrorReporter _errorReporter;

        protected ErrorReportingBackgroundService(
            ILogger logger,
            IServiceErrorReporter errorReporter)
        {
            _logger = logger;
            _errorReporter = errorReporter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{GetType().Name} запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.LogError(ex, $"Ошибка в {GetType().Name}");
                    
                    try
                    {
                        var reportSent = await _errorReporter.SendReportAsync(ex);
                        if (!reportSent)
                        {
                            _logger.LogError("Не удалось отправить отчет об ошибке");
                        }
                    }
                    catch (Exception reportEx)
                    {
                        _logger.LogError(reportEx, "Ошибка при отправке отчета об ошибке");
                    }
                }

                await Task.Delay(GetDelayPeriod(), stoppingToken);
            }
        }

        /// <summary>
        /// Основная работа фонового сервиса
        /// </summary>
        protected abstract Task DoWorkAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Период задержки между итерациями
        /// </summary>
        protected abstract TimeSpan GetDelayPeriod();
    }
}

