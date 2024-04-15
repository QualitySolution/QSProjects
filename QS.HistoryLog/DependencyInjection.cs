using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
using QS.Project.Core;
using System;

namespace QS.HistoryLog {
	public static class DependencyInjection 
	{
		[Obsolete("Необходимо добавить поддержку внедрения зависимостей в HistoryLog")]
		public static IServiceCollection AddStaticHistoryTracker(this IServiceCollection services) {
			services.AddSingleton<OnDatabaseInitialization>((provider) => {
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				HistoryMain.Enable(connectionStringBuilder);
				return new OnDatabaseInitialization();
			});
			return services;
		}
	}
}
