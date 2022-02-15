using QS.DomainModel.UoW;
using QS.Report.Domain;
using System.Collections.Generic;

namespace QS.Report.Repository
{
	public class UserPrintingRepository : IUserPrintingRepository
	{
		public IList<UserSelectedPrinter> GetUserSelectedPrinters(IUnitOfWork uow, int userId) =>
			uow.Session.QueryOver<UserSelectedPrinter>().Where(x => x.User.Id == userId).List();

		public UserPrintSettings GetUserPrintSettings(IUnitOfWork uow, int userId) =>
			uow.Session.QueryOver<UserPrintSettings>().Where(x => x.User.Id == userId).SingleOrDefault();
	}
}

