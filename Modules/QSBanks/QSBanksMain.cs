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
				new OrmObjectMaping(typeof(Bank), typeof(BankDlg)),
				new OrmObjectMaping(typeof(Account), typeof(AccountDlg)),
			};
		}

	}

	public interface IAccountOwner
	{
		Account DefaultAccount { get; set;}
		IList<Account> Accounts { get; set;}
	}
}

