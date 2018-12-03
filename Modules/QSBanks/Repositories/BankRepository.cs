using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace QSBanks.Repositories
{
	public static class BankRepository
	{
		public static IList<Bank> GetAllBanks(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Bank>().List<Bank>();
		}

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

		public static IList<BankRegion> GetAllBankRegions(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<BankRegion>().List<BankRegion>();
		}
	}
}

