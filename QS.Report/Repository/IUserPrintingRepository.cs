using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report.Domain;

namespace QS.Report.Repository
{
	public interface IUserPrintingRepository
	{
		IList<UserSelectedPrinter> GetUserSelectedPrinters(IUnitOfWork uow, int userId);
		UserPrintSettings GetUserPrintSettings(IUnitOfWork uow, int userId);
	}
}