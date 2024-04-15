using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using System.Data.Common;

namespace QS.Project.DB {
	public interface IOrmConfig
    {
        Configuration Configuration { get; }
        ISessionFactory SessionFactory { get; }
        ISession OpenSession(DbConnection connection);
        ISession OpenSession(IInterceptor interceptor = null);
        PersistentClass FindMappingByFullClassName(string clazz);
        PersistentClass FindMappingByShortClassName(string clazz);
    }
}
