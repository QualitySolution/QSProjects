using FluentNHibernate.Mapping;
using QS.Extensions.Observable.Collections.List;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.HibernateMapping
{
	public class EntityWithObservableListMap : ClassMap<EntityWithObservableList>
	{
		public EntityWithObservableListMap()
		{
			Table("entities_with_observable_list");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");

			HasMany(x => x.Items)
				.KeyColumn("entity_id")
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.CollectionType<ObservableList<TestItem>>();
		}
	}
}

