using NHibernate;

namespace QS.Project.DB {
	public class DefaultSessionProvider : ISessionProvider
	{
		public DefaultSessionProvider()
		{
		}

		public virtual ISession OpenSession()
		{
			ISession session = OrmConfig.Config.SessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}
	}
}
