using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QS.Project.DB {
	public interface IOrmSettings {
		IDataBaseInfo DataBaseInfo { get; }
		IEnumerable<Assembly> GetMappingAssemblies();
		MySQLConfiguration GetDatabaseConfiguration(IServiceProvider provider);
		void ExposeConfiguration(Configuration configuration);
	}
}
