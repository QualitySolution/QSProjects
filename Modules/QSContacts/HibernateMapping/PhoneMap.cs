using System;
using FluentNHibernate.Mapping;

namespace QSContacts.HMap
{
	public class PhoneMap : ClassMap<Phone>
	{
		public PhoneMap ()
		{
			Table("phones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Number).Column("number");
			Map(x => x.DigitsNumber).Column("digits_number");
			Map(x => x.Additional).Column("additional");

			References(x => x.NumberType).Column("type_id").Not.LazyLoad();
		}
	}
}

