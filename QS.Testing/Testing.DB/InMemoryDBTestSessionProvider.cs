using NHibernate;
using NHibernate.Tool.hbm2ddl;
using QS.Project.DB;

namespace QS.Testing.DB
{
	public class InMemoryDBTestSessionProvider : ISessionProvider
	{
		// имя пишем полностью: короткое Configuration перехватывает пространство имён QS.Configuration
		private readonly NHibernate.Cfg.Configuration configuration;
		private readonly ISessionFactory sessionFactory;
		private bool useSameDb = false;
		ISession lastSession;

		public InMemoryDBTestSessionProvider(NHibernate.Cfg.Configuration configuration, ISessionFactory sessionFactory)
		{
			this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
			this.sessionFactory = sessionFactory ?? throw new System.ArgumentNullException(nameof(sessionFactory));
		}

		public bool UseSameDB { get => useSameDb; 
			set { 
				useSameDb = value;
				if(useSameDb == false)
					lastSession = null;
			} 
		}

		public ISession OpenSession()
		{
			if(lastSession != null && lastSession.IsOpen && UseSameDB)
				return sessionFactory.WithOptions().Connection(lastSession.Connection).OpenSession();

			lastSession = sessionFactory.OpenSession();
			//Создаем схему таблиц БД
			new SchemaExport(configuration).Execute(false, true, false, lastSession.Connection, null);

			return lastSession;
		}
	}
}
