using System;
using System.Collections.Generic;
using QSOrmProject;
using QSOrmProject.DomainMapping;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static List<IOrmObjectMapping> GetModuleMaping ()
		{
			return new List<IOrmObjectMapping> {
				OrmObjectMapping<Bank>.Create().Dialog<BankDlg>().JournalFilter<BankFilter>()
					.DefaultTableView().SearchColumn("БИК", x=> x.Bik).SearchColumn("Имя", x => x.Name).SearchColumn("Город", x => x.City).End(),
				OrmObjectMapping<Account>.Create().Dialog (typeof(AccountDlg)),
				OrmObjectMapping<BankRegion>.Create()
			};
		}

	}

}

