using Autofac;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Data.Common;

namespace QS.Project.DB {
	[Obsolete("Вместо него необходимо использовать IOrmConfig из контейнера")]
	public static class OrmConfig
	{
		public static ILifetimeScope Scope { private get; set; }
		public static IOrmConfig Config => Scope.Resolve<IOrmConfig>();

		private static void CheckConfig() 
		{
			if(Config == null) {
				throw new InvalidOperationException($"Вы используете устаревший класс! " +
					$"Для его конфигурации установите в свойство {nameof(Config)} " +
					$"экземпляр {nameof(IOrmConfig)} при инициализации приложения");
			}
		}

		public static ISession OpenSession(IInterceptor interceptor = null)
		{
			CheckConfig();
			return Config.OpenSession(interceptor);
		}

		public static ISession OpenSession(DbConnection connection)
		{
			CheckConfig();
			return Config.OpenSession(connection);
		}

		public static Configuration NhConfig => Config.Configuration;

		public static NHibernate.Mapping.PersistentClass FindMappingByShortClassName(string clazz)
		{
			CheckConfig();
			return Config.FindMappingByShortClassName(clazz);
		}

		public static NHibernate.Mapping.PersistentClass FindMappingByFullClassName(string clazz)
		{
			CheckConfig();
			return Config.FindMappingByFullClassName(clazz);
		}
	}
}
