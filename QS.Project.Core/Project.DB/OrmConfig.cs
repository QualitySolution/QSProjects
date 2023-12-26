using NHibernate;
using NHibernate.Cfg;
using System;
using System.Data.Common;

namespace QS.Project.DB {
	[Obsolete("Вместо него необходимо использовать IOrmConfig из контейнера")]
	public static class OrmConfig
	{
		public static IOrmConfig Config { get; set; }

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

		public static Configuration NhConfig {
			get {
				CheckConfig();
				return Config.NhConfig;
			}
			set {
				CheckConfig();
				Config.NhConfig = value;
			}
		}

		/// <summary>
		/// Настройка Nhibernate только с Fluent конфигураций.
		/// </summary>
		/// <param name="assemblies">Assemblies.</param>
		public static void ConfigureOrm(FluentNHibernate.Cfg.Db.IPersistenceConfigurer database, System.Reflection.Assembly[] assemblies, Action<Configuration> exposeConfiguration = null)
		{
			CheckConfig();
			Config.ConfigureOrm(database, assemblies, exposeConfiguration);
		}

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
