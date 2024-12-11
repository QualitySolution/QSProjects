using System;
using QS.Banks.Domain;
using QS.Deletion;
using QS.Deletion.Configuration;

namespace QS.Banks
{
	public static class ConfigureDeletionBanks
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void ConfigureDeletion(DeleteConfiguration configuration)
		{
			logger.Info("Настройка параметров удаления в модуле Banks...");

			configuration.AddHibernateDeleteInfo<BankRegion>()
				.AddDeleteDependence<Bank>(item => item.Region);

			configuration.AddHibernateDeleteInfo<Bank>()
				.AddDeleteDependence<Account>(item => item.InBank)
				.AddDeleteDependenceFromCollection(item => item.CorAccounts);

			configuration.AddHibernateDeleteInfo<Account>()
				.AddClearDependence<Bank>(item => item.DefaultCorAccount);
		}
	}
}
