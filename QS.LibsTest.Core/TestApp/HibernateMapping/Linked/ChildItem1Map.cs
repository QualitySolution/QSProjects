using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain.Linked;

namespace QS.Test.TestApp.HibernateMapping
{
	public class ChildItem1Map : ClassMap<AlsoDeleteItem>
	{
		public ChildItem1Map()
		{
			Id(x => x.Id);

			References(x => x.Root);
		}
	}
}
