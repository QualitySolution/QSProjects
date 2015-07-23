using QSOrmProject;
using System.Collections.Generic;

namespace QSBanks.Repository
{
	public static class BankRepository
	{
		public static IList<Bank> ActiveBanks (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Bank> ().Where (b => !b.Deleted).List<Bank> ();
		}
	}
}

