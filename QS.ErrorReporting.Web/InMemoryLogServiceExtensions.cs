using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QS.ErrorReporting
{
	public static class InMemoryLogServiceExtensions
	{
		public static IServiceCollection AddInMemoryLogService(this IServiceCollection services)
		{
			services.AddSingleton<InMemoryLogService>();
			services.AddSingleton<IAsyncLogService>(provider => provider.GetRequiredService<InMemoryLogService>());
			services.AddSingleton<ILoggerProvider>(provider => provider.GetRequiredService<InMemoryLogService>());
			return services;
		}
	}
}
