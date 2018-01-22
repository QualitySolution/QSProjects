using System;
using FluentNHibernate.Mapping;

namespace QSHistoryLog.HibernateMapping
{
	public class FieldChangeMap : ClassMap<FieldChange>
	{
		public FieldChangeMap()
		{
			Table("history_changes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Type).Column("type").CustomType<FieldChangeTypeStringType>();
			Map(x => x.Path).Column("path");
			Map(x => x.OldValue).Column("old_value");
			Map(x => x.OldId).Column("old_id");
			Map(x => x.NewValue).Column("new_value");
			Map(x => x.NewId).Column("new_id");

			References(x => x.ChangeSet).Column("changeset_id");
		}
	}
}
