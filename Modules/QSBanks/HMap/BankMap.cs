using System;
using FluentNHibernate.Mapping;

namespace QSBanks.HMap
{
	public class BankMap : ClassMap<Bank>
	{
		public BankMap ()
		{
			Table("banks");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Bik).Column("bik");
			Map(x => x.CorAccount).Column("cor_account");
			Map(x => x.City).Column("city");
			Map(x => x.Deleted).Column("deleted");

			References(x => x.Region).Column("region_id");
		}
	}
}

