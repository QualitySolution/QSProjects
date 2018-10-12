using System;
using System.Linq;
using QSOrmProject;

namespace QS.Helpers
{
	public static class NHibernateHelper
	{
		public static NHibernate.Mapping.PersistentClass FindMappingByShortClassName(string clazz)
		{
			return OrmMain.OrmConfig.ClassMappings
				.FirstOrDefault(c => c.MappedClass.Name == clazz);
		}
	}
}
