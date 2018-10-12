using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace QSContacts
{
	public static class PhoneTypeRepository
	{
		public static IList<PhoneType> GetPhoneTypes (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType> ().List<PhoneType> ();
		}
	}
}