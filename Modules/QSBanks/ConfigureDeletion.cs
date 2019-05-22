using QS.Deletion;

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
				.AddDeleteDependence<Account>(item => item.InBank)
				.AddDeleteDependenceFromCollection(item => item.CorAccounts);

			DeleteConfig.AddHibernateDeleteInfo<Account>()
				.AddClearDependence<Bank>(item => item.DefaultCorAccount);
		}
	}
}

