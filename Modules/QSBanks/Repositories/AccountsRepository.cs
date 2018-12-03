using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace QSBanks.Repositories
{
	public static class AccountsRepository
	{
		public static IList<Account> GetActiveAccounts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Account>().Where(x => !x.Inactive).List<Account>();
		}

		public static IList<Account> GetAllAccounts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Account>().List<Account>();
		}
	}
}
