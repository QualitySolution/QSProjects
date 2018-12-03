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
			Map(x => x.City).Column("city");
			Map(x => x.Deleted).Column("deleted");

			References(x => x.Region).Column("region_id");
			References(x => x.DefaultCorAccount).Column("default_cor_account_id").Cascade.All();
			HasMany(x => x.CorAccounts).Cascade.AllDeleteOrphan().Inverse().KeyColumn("bank_id");
		}
	}
}

