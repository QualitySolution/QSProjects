using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.HibernateMapping
{
	public class TestItemMap : ClassMap<TestItem>
	{
		public TestItemMap()
		{
			Table("test_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Value).Column("value");
		}
	}
}

