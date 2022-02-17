using System.ComponentModel.Design;

namespace QS.Validation
{
	public static class ServiceContainerExtensions
	{
		public static void AddService<T>(this IServiceContainer context, T service) where T : class
		{
			context.AddService(typeof(T), service);
		}

		public static void AddService<T>(this IServiceContainer context, T service, bool promote) where T : class
		{
			context.AddService(typeof(T), service, promote);
		}

		public static void AddService<T>(this IServiceContainer context, ServiceCreatorCallback callback) where T : class
		{
			context.AddService(typeof(T), callback);
		}

		public static void AddService<T>(this IServiceContainer context, ServiceCreatorCallback callback, bool promote) where T : class
		{
			context.AddService(typeof(T), callback, promote);
		}
	}
}
