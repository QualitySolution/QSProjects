using System;
using FluentNHibernate.Mapping;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog.HibernateMapping
{
	public class ChangeSetMap : ClassMap<ChangeSet>
	{
		public ChangeSetMap()
		{
			Table("history_changeset");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.UserLogin).Column("user_login");
			Map(x => x.ActionName).Column("action_name");

			References(x => x.User).Column("user_id").Not.LazyLoad();

			HasMany(x => x.Entities).Inverse().LazyLoad().KeyColumn("changeset_id");
		}
	}
}
