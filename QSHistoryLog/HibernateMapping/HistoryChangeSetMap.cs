using FluentNHibernate.Mapping;
using QSHistoryLog.Domain;

namespace QSHistoryLog.HibernateMapping
{
	public class HistoryChangeSetMap : ClassMap<HistoryChangeSet>
	{
		public HistoryChangeSetMap()
		{
			Table("history_changeset");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ChangeTime).Column("datetime");
			Map(x => x.Operation).Column("operation").CustomType<ChangeSetTypeStringType>();
			Map(x => x.ObjectName).Column("object_name");
			Map(x => x.ItemId).Column("object_id");
			Map(x => x.ItemTitle).Column("object_title");
			Map(x => x.UserId).Column("user_id");

			//References(x => x.User).Column("user_id").Not.LazyLoad();

			HasMany(x => x.Changes).Cascade.AllDeleteOrphan().Inverse().Not.LazyLoad().KeyColumn("changeset_id");
		}
	}
}
