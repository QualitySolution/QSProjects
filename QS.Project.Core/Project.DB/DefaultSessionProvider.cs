using NHibernate;

namespace QS.Project.DB {
	public class DefaultSessionProvider : ISessionProvider
	{
		private readonly ISessionFactory sessionFactory;

		public DefaultSessionProvider(ISessionFactory sessionFactory)
		{
			this.sessionFactory = sessionFactory ?? throw new System.ArgumentNullException(nameof(sessionFactory));
		}

		public virtual ISession OpenSession()
		{
			ISession session = sessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}
	}
}
