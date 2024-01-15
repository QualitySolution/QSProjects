using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using System;
using System.Data.Common;
using System.Linq;

namespace QS.Project.DB {
	public class DefaultOrmConfig : IOrmConfig
    {
		public DefaultOrmConfig(ISessionFactory sessionFactory, Configuration configuration) {
			SessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
			Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

        public ISessionFactory SessionFactory { get; private set; }
		public Configuration Configuration { get; private set; }

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

        public PersistentClass FindMappingByShortClassName(string clazz)
        {
            return Configuration.ClassMappings
                .FirstOrDefault(c => c.MappedClass.Name == clazz);
        }

        public PersistentClass FindMappingByFullClassName(string clazz)
        {
            return Configuration.ClassMappings
                .FirstOrDefault(c => c.MappedClass.FullName == clazz);
        }
    }
}
