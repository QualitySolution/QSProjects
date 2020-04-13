using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain.Linked;

namespace QS.Test.TestApp.HibernateMapping
{
	public class ChildItem2Map : ClassMap<RootDeleteItem>
	{
		public ChildItem2Map()
		{
			Id(x => x.Id);

			Map(x => x.Text);
		}
	}
}
