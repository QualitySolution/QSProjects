using System;
using FluentNHibernate.Mapping;

namespace QSContacts.HMap
{
	public class EmailMap : ClassMap<Email>
	{
		public EmailMap ()
		{
			Table("emails");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Address).Column("address");

			References(x => x.EmailType).Column("type_id");
		}
	}
}

