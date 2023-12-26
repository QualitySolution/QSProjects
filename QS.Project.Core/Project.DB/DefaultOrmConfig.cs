using Autofac;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using QS.DomainModel.Tracking;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace QS.Project.DB {
	public class DefaultOrmConfig : IOrmConfig
    {
        private FluentConfiguration fluenConfig;
		private Configuration nhConfig;
		private readonly ILifetimeScope scope;

		public DefaultOrmConfig(ILifetimeScope scope) {
			this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

        public ISessionFactory SessionFactory { get; private set; }

        public Configuration NhConfig {
            get {
                if(nhConfig == null && fluenConfig != null)
                    nhConfig = fluenConfig.BuildConfiguration();
                return nhConfig;
            }
            set => nhConfig = value;
        }

        public ISession OpenSession(IInterceptor interceptor = null)
        {
            ISession session = interceptor == null ? SessionFactory.OpenSession() : SessionFactory.WithOptions().Interceptor(interceptor).OpenSession();
            session.FlushMode = FlushMode.Commit;
            return session;
        }

        public ISession OpenSession(DbConnection connection)
        {
            return SessionFactory.WithOptions()
                .Connection(connection)
                .FlushMode(FlushMode.Commit)
                .OpenSession();
        }

        /// <summary>
        /// Настройка Nhibernate только с Fluent конфигураций.
        /// </summary>
        public void ConfigureOrm(IPersistenceConfigurer database, Assembly[] assemblies, Action<Configuration> exposeConfiguration = null)
        {
            fluenConfig = Fluently.Configure().Database(database);

			var conventions = scope.ResolveOptional<IEnumerable<IConvention>>();

			fluenConfig.Mappings(m => {
				if(conventions != null && conventions.Any()) {
					m.FluentMappings.Conventions.Add(conventions.ToArray());
				}
				foreach(var ass in assemblies) {
                    m.FluentMappings.AddFromAssembly(ass);
                }
            });

			var tracker = scope.ResolveOptional<GlobalUowEventsTracker>();
			if(tracker != null) {
				var listeners = new[] { tracker };
				fluenConfig.ExposeConfiguration(cfg => {
					cfg.AppendListeners(NHibernate.Event.ListenerType.PostLoad, listeners);
					cfg.AppendListeners(NHibernate.Event.ListenerType.PreLoad, listeners);
					cfg.AppendListeners(NHibernate.Event.ListenerType.PostDelete, listeners);
					cfg.AppendListeners(NHibernate.Event.ListenerType.PostUpdate, listeners);
					cfg.AppendListeners(NHibernate.Event.ListenerType.PostInsert, listeners);
				});
			}

            if(exposeConfiguration != null)
                fluenConfig.ExposeConfiguration(exposeConfiguration);

            SessionFactory = fluenConfig.BuildSessionFactory();
        }

        public PersistentClass FindMappingByShortClassName(string clazz)
        {
            return NhConfig.ClassMappings
                .FirstOrDefault(c => c.MappedClass.Name == clazz);
        }

        public PersistentClass FindMappingByFullClassName(string clazz)
        {
            return NhConfig.ClassMappings
                .FirstOrDefault(c => c.MappedClass.FullName == clazz);
        }
    }
}
