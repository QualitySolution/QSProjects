using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace QS.Utilities.Extensions {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddService<TService, TImplementation>(
			this IServiceCollection services,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
			where TService : class
			where TImplementation : class, TService {
			return services.AddService(typeof(TService), typeof(TImplementation), serviceLifetime);
		}
		
		public static IServiceCollection AddService<TService, TImplementation>(
			this IServiceCollection services,
			Func<IServiceProvider, TImplementation> implementationFactory,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
			where TService : class
			where TImplementation : class, TService {
			return services.AddService(typeof(TService), implementationFactory, serviceLifetime);
		}
		
		public static IServiceCollection AddService<TService>(
			this IServiceCollection services,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
			where TService : class {
			return services.AddService(typeof(TService), serviceLifetime);
		}

		public static IServiceCollection AddService<TService>(
			this IServiceCollection services,
			Func<IServiceProvider, TService> implementationFactory,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
			where TService : class {
			return services.AddService(typeof(TService), implementationFactory, serviceLifetime);
		}
		
		public static IServiceCollection AddService(
			this IServiceCollection services,
			Type serviceType,
			Type implementationType,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			switch(serviceLifetime)
			{
				case ServiceLifetime.Transient:
					services.AddTransient(serviceType, implementationType);
					break;
				case ServiceLifetime.Singleton:
					services.AddSingleton(serviceType, implementationType);
					break;
				default:
					services.AddScoped(serviceType, implementationType);
					break;
			}
			
			return services;
		}
		
		public static IServiceCollection AddService(
			this IServiceCollection services,
			Type serviceType,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) {
			return services.AddService(serviceType, serviceType, serviceLifetime);
		}
		
		public static IServiceCollection AddServicesEndsWith(
			this IServiceCollection services,
			Assembly assembly,
			string nameEndsWith,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
			bool asSelf = false) {

			var types = assembly.GetTypes()
				.Where(t => t.Name.EndsWith(nameEndsWith));
			
			if(!asSelf) {
				foreach(var implementationType in types) {
					var repositoryInterface =
						implementationType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{implementationType.Name}");
					
					if(repositoryInterface != null)
					{
						services.AddService(repositoryInterface, implementationType, serviceLifetime);
					}
				}
			}
			else {
				foreach(var implementationType in types)
				{
					services.AddService(implementationType, serviceLifetime);
				}
			}

			return services;
		}

		private static IServiceCollection AddService(
			this IServiceCollection services,
			Type serviceType,
			Func<IServiceProvider, object> implementationFactory,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			switch(serviceLifetime)
			{
				case ServiceLifetime.Transient:
					services.AddTransient(serviceType, implementationFactory);
					break;
				case ServiceLifetime.Singleton:
					services.AddSingleton(serviceType, implementationFactory);
					break;
				default:
					services.AddScoped(serviceType, implementationFactory);
					break;
			}
		
			return services;
		}
	}
}
