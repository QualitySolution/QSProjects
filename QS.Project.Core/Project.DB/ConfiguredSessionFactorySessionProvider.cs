using NHibernate;
using System;

namespace QS.Project.DB {
	public class ConfiguredSessionFactorySessionProvider : ISessionProvider
    {
        public ConfiguredSessionFactorySessionProvider(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        private readonly ISessionFactory sessionFactory;
        public virtual ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
            return session;
        }
    }
}
