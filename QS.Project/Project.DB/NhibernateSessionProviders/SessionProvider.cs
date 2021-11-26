using System;
using NHibernate;

namespace QS.Project.DB
{
	public class SessionProvider : ISessionProvider
	{
		private readonly ISessionFactory _sessionFactory;

		public SessionProvider(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
		}

		public virtual ISession OpenSession()
		{
			var session = _sessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}
	}
}
