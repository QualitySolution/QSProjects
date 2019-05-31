using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using QS.Project.DB;

namespace QS.DB
{
	public class InMemoryDBTestSessionProvider : DefaultSessionProvider
	{
		readonly Configuration configuration;
		public InMemoryDBTestSessionProvider(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public override ISession OpenSession()
		{
			var session = base.OpenSession();
			//Создаем схему таблиц БД
			new SchemaExport(configuration).Execute(false, true, false, session.Connection, null);
			return session;
		}
	}
}
