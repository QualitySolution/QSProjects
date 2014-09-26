using System;
using System.Collections.Generic;
using QSOrmProject;

namespace QSBanks
{
	public static class QSBanksMain
	{

		public static List<OrmObjectMaping> GetModuleMaping()
		{
			return new List<OrmObjectMaping>
			{
				new OrmObjectMaping(typeof(Bank), typeof(BankDlg), "{QSBanks.Bank} Bik[БИК]; Name[Имя]; City[Город];"),
				new OrmObjectMaping(typeof(Account), typeof(AccountDlg)),
			};
		}

	}

}

