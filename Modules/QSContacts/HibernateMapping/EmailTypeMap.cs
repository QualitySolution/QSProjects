using System;
using FluentNHibernate.Mapping;

namespace QSContacts.HMap
{
	public class EmailTypeMap : ClassMap<EmailType>
	{
		public EmailTypeMap ()
		{
			Table("email_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}

