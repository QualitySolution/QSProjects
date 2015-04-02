using System;
using System.Collections.Generic;
using QSOrmProject;

namespace QSBanks
{
	public static class QSBanksMain
	{

		public static List<OrmObjectMapping> GetModuleMaping ()
		{
			return new List<OrmObjectMapping> {
				new OrmObjectMapping (typeof(Bank), typeof(BankDlg), typeof(BankFilter), "{QSBanks.Bank} Bik[БИК]; Name[Имя]; City[Город];", new string[] { "Bik", "Name", "City" }),
				new OrmObjectMapping (typeof(Account), typeof(AccountDlg)),
			};
		}

	}

}

