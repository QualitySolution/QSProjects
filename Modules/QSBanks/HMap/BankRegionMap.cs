using System;
using FluentNHibernate.Mapping;

namespace QSBanks.HMap
{
	public class BankRegionMap : ClassMap<BankRegion>
	{
		public BankRegionMap ()
		{
			Table("bank_regions");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Region).Column("region");
			Map(x => x.RegionNum).Column("region_num");
			Map(x => x.City).Column("city");
		}
	}
}

