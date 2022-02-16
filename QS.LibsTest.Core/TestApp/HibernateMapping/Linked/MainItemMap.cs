using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain.Linked;

namespace QS.Test.TestApp.HibernateMapping
{
	public class MainItemMap : ClassMap<DependDeleteItem>
	{
		public MainItemMap()
		{
			Id(x => x.Id);

			References(x => x.CleanLink);
			References(x => x.DeleteLink);
		}
	}
}
