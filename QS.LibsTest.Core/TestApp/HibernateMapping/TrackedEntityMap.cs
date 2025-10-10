using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.HibernateMapping
{
	public class TrackedEntityMap : ClassMap<TrackedEntity>
	{
		public TrackedEntityMap()
		{
			Id(x => x.Id);

			Map(x => x.Name);
			Map(x => x.Description);
			
			References(x => x.AnotherEntity);
		}
	}
}
