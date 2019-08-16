using System;
using FluentNHibernate.Mapping;

namespace QS.Contacts.HMap
{
	public class EmailMap : ClassMap<Email>
	{
		public EmailMap ()
		{
			Table("emails");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Address).Column("address").Not.Nullable();

			References(x => x.EmailType).Column("type_id");
		}
	}
}

