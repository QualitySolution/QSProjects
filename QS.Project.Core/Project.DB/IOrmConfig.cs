using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using System;
using System.Data.Common;
using System.Reflection;

namespace QS.Project.DB {
	public interface IOrmConfig
    {
        Configuration NhConfig { get; set; }
        ISessionFactory SessionFactory { get; }
        
        void ConfigureOrm(IPersistenceConfigurer database, Assembly[] assemblies, Action<Configuration> exposeConfiguration = null);
        ISession OpenSession(DbConnection connection);
        ISession OpenSession(IInterceptor interceptor = null);
        PersistentClass FindMappingByFullClassName(string clazz);
        PersistentClass FindMappingByShortClassName(string clazz);
    }
}
