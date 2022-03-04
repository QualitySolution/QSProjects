using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestDomainMapping.TestDomain
{
	public class SimpleEntityMap : ClassMap<SimpleEntity>
	{
		public SimpleEntityMap()
		{
			Id(x => x.Id);

			Map(x => x.Text);
		}
	}
}
