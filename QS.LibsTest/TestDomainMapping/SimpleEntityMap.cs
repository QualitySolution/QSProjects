using FluentNHibernate.Mapping;
using QS.Test.TestDomain;

namespace QS.Test.TestDomainMapping.TestDomain
{
	public class SimpleEntityMap : ClassMap<SimpleEntity>
	{
		public SimpleEntityMap()
		{
			Id(x => x.Id);
		}
	}
}
