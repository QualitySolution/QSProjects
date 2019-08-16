using System;
using QS.Banks.Domain;
using QS.Deletion;

namespace QS.Banks
{
	public static class ConfigureDeletionBanks
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void ConfigureDeletion()
		{
			logger.Info("Настройка параметров удаления в модуле Banks...");

			DeleteConfig.AddHibernateDeleteInfo<BankRegion>()
				.AddDeleteDependence<Bank>(item => item.Region);

			DeleteConfig.AddHibernateDeleteInfo<Bank>()
				.AddDeleteDependence<Account>(item => item.InBank)
				.AddDeleteDependenceFromCollection(item => item.CorAccounts);

			DeleteConfig.AddHibernateDeleteInfo<Account>()
				.AddClearDependence<Bank>(item => item.DefaultCorAccount);
		}
	}
}
