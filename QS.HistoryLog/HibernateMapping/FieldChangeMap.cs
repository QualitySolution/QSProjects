using System;
using FluentNHibernate.Mapping;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog.HibernateMapping
{
	public class FieldChangeMap : ClassMap<FieldChange>
	{
		public FieldChangeMap()
		{
			Table("history_changes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Type).Column("type").CustomType<FieldChangeTypeStringType>();
			Map(x => x.Path).Column("field_name");
			Map(x => x.OldValue).Column("old_value");
			Map(x => x.OldId).Column("old_id");
			Map(x => x.NewValue).Column("new_value");
			Map(x => x.NewId).Column("new_id");

			References(x => x.Entity).Column("changed_entity_id");
		}
	}
}
