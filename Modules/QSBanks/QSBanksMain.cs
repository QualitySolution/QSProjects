using System;
using System.Collections.Generic;
using QSOrmProject;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static List<IOrmObjectMapping> GetModuleMaping ()
		{
			return new List<IOrmObjectMapping> {
				new OrmObjectMapping<Bank> (typeof(BankDlg), typeof(BankFilter), "{QSBanks.Bank} Bik[БИК]; Name[Имя]; City[Город];", new string[] { "Bik", "Name", "City" }),
				new OrmObjectMapping<Account> (typeof(AccountDlg)),
				new OrmObjectMapping<BankRegion> (null)
			};
		}

	}

}

