using System;
using FluentNHibernate.Mapping;

namespace QSContacts.HMap
{
	public class PhoneTypeMap : ClassMap<PhoneType>
	{
		public PhoneTypeMap ()
		{
			Table("phone_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}

