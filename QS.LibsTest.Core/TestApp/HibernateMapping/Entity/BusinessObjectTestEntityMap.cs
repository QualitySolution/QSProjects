using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain.Entity;

namespace QS.Test.TestApp.HibernateMapping.Entity
{
	public class BusinessObjectTestEntityMap : ClassMap<BusinessObjectTestEntity>
	{
		public BusinessObjectTestEntityMap()
		{
			Id(x => x.Id);
		}
	}
}
