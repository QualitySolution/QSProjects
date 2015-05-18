using System;
using System.Collections.Generic;
using QSOrmProject;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static List<OrmObjectMapping> GetModuleMaping ()
		{
			return new List<OrmObjectMapping> {
				new OrmObjectMapping (typeof(Bank), typeof(BankDlg), typeof(BankFilter), "{QSBanks.Bank} Bik[БИК]; Name[Имя]; City[Город];", new string[] { "Bik", "Name", "City" }),
				new OrmObjectMapping (typeof(Account), typeof(AccountDlg)),
				new OrmObjectMapping (typeof(BankRegion), null)
			};
		}

	}

}

