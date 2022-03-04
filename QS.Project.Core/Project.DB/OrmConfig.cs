using System;
using System.Data.Common;
using System.Linq;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using QS.DomainModel.Tracking;

namespace QS.Project.DB
{
	public static class OrmConfig
	{
		private static Configuration nhConfig;
		internal static FluentConfiguration fluenConfig;
		internal static ISessionFactory Sessions;

		public static ISession OpenSession(IInterceptor interceptor = null)
		{
			ISession session = interceptor == null ? Sessions.OpenSession() : Sessions.WithOptions().Interceptor(interceptor).OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}

		public static ISession OpenSession(DbConnection connection)
		{
			return Sessions.WithOptions()
				.Connection(connection)
				.FlushMode(FlushMode.Commit)
				.OpenSession();
		}

		public static Configuration NhConfig {
			get {
				if(nhConfig == null && fluenConfig != null)
					nhConfig = fluenConfig.BuildConfiguration();
				return nhConfig;
			}
			set {
				nhConfig = value;
			}
		}

		/// <summary>
		/// Настройка Nhibernate только с Fluent конфигураций.
		/// </summary>
		/// <param name="assemblies">Assemblies.</param>
		public static void ConfigureOrm(FluentNHibernate.Cfg.Db.IPersistenceConfigurer database, System.Reflection.Assembly[] assemblies, Action<Configuration> exposeConfiguration = null)
		{
			fluenConfig = Fluently.Configure().Database(database);

			fluenConfig.Mappings(m => {
				foreach(var ass in assemblies) {
					m.FluentMappings.AddFromAssembly(ass);
				}
			});

			var trackerListener = new GlobalUowEventsTracker();
			fluenConfig.ExposeConfiguration(cfg => {
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostLoad, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PreLoad, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostDelete, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostUpdate, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostInsert, new[] { trackerListener });
			});

			if(exposeConfiguration != null)
				fluenConfig.ExposeConfiguration(exposeConfiguration);

			Sessions = fluenConfig.BuildSessionFactory();
		}

		public static String GetDBTableName(Type objectClass)
		{
			return nhConfig.GetClassMapping(objectClass).RootTable.Name;
		}

		public static NHibernate.Mapping.PersistentClass FindMappingByShortClassName(string clazz)
		{
			return NhConfig.ClassMappings
				.FirstOrDefault(c => c.MappedClass.Name == clazz);
		}

		public static NHibernate.Mapping.PersistentClass FindMappingByFullClassName(string clazz)
		{
			return NhConfig.ClassMappings
				.FirstOrDefault(c => c.MappedClass.FullName == clazz);
		}
	}
}
