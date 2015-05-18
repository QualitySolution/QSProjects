using System;
using System.Collections.Generic;
using QSOrmProject.Deletion;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		public static void ConfigureDeletion()
		{
			logger.Info ("Настройка параметров удаления в модуле Banks...");

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(BankRegion),
				SqlSelect = "SELECT id, region, city FROM @tablename ",
				DisplayString = "{1} - {2}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<Bank> (item => item.Region)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Bank),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<Account> (item => item.InBank)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Account),
				SqlSelect = "SELECT id, name, number FROM @tablename ",
				DisplayString = "{1}({2})"
			}.FillFromMetaInfo ()
			);
		}
	}
}

