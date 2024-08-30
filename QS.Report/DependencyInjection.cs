using Microsoft.Extensions.DependencyInjection;

namespace QS.Report {
	public static class DependencyInjection {
		public static IServiceCollection AddReportsCore(this IServiceCollection services) {

			services.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>();

			return services;
		}
	}
}
