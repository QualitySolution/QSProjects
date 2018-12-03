using System;
using FluentNHibernate.Mapping;

namespace QSBanks.HMap
{
	public class CorAccountMap : ClassMap<CorAccount>
	{
		public CorAccountMap()
		{
			Table("banks_cor_accounts");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CorAccountNumber).Column("cor_account_number");
			References(x => x.InBank).Column("bank_id");
		}
	}
}
