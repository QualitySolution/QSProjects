using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using QS.Project.DB;

namespace QS.Testing.DB
{
	public class InMemoryDBTestSessionProvider : DefaultSessionProvider
	{
		readonly Configuration configuration;

		private bool useSameDb = false;
		ISession lastSession;

		public InMemoryDBTestSessionProvider(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public bool UseSameDB { get => useSameDb; 
			set { 
				useSameDb = value;
				if(useSameDb == false)
					lastSession = null;
			} 
		}

		public override ISession OpenSession()
		{
			if(lastSession != null && lastSession.IsOpen && UseSameDB)
				return OrmConfig.OpenSession(lastSession.Connection);

			lastSession = base.OpenSession();
			//Создаем схему таблиц БД
			new SchemaExport(configuration).Execute(false, true, false, lastSession.Connection, null);

			return lastSession;
		}
	}
}
