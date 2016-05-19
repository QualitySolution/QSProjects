using System;
using QSOrmProject.Deletion;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		public static void ConfigureDeletion()
		{
			logger.Info ("Настройка параметров удаления в модуле Banks...");

			DeleteConfig.AddHibernateDeleteInfo<BankRegion>()
				.AddDeleteDependence<Bank>(item => item.Region);

			DeleteConfig.AddHibernateDeleteInfo<Bank>()
				.AddDeleteDependence<Account>(item => item.InBank);

			DeleteConfig.AddHibernateDeleteInfo<Account>();
		}
	}
}

