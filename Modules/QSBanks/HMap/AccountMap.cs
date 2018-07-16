using System;
using FluentNHibernate.Mapping;

namespace QSBanks.HMap
{
	public class AccountMap : ClassMap<Account>
	{
		public AccountMap ()
		{
			Table("accounts");

			//ВАЖНО : Если будет включена ленивая загрузка перестанет работать слонер объекта в подчиненном UoW.
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Number).Column("number");
			Map(x => x.Code1c).Column("code_1c");
			Map(x => x.Inactive).Column("inactive");
			Map(x => x.IsDefault).Column("is_default");

			References(x => x.InBank).Column("bank_id");
		}
	}
}

