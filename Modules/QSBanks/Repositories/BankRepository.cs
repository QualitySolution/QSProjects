using QSOrmProject;
using System.Collections.Generic;
using System;

namespace QSBanks.Repository
{
	public static class BankRepository
	{
		public static IList<Bank> ActiveBanks (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Bank> ().Where (b => !b.Deleted).List<Bank> ();
		}

		public static Bank GetBankByBik (IUnitOfWork uow, string bik)
		{
			if (String.IsNullOrEmpty (bik))
				return null;
			return uow.Session.QueryOver<Bank> ()
				.Where (b => b.Bik == bik)
				.Take (1)
				.SingleOrDefault ();
		}
	}
}

