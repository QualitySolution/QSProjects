using FluentNHibernate.Mapping;
using QS.Test.TestDomain;

namespace QS.Test.TestDomainMapping.TestDomain
{
	public class BusinessObjectTestEntityMap : ClassMap<BusinessObjectTestEntity>
	{
		public BusinessObjectTestEntityMap()
		{
			Id(x => x.Id);
		}
	}
}
