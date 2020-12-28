using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using QS.DomainModel.Tracking;

namespace QS.Project.DB
{
    public class DefaultOrmConfig : IOrmConfig
    {
        private FluentConfiguration fluenConfig;

        public ISessionFactory SessionFactory { get; private set; }

        private Configuration nhConfig;
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

            SessionFactory = fluenConfig.BuildSessionFactory();
        }

        public string GetDBTableName(Type objectClass)
        {
            return nhConfig.GetClassMapping(objectClass).RootTable.Name;
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