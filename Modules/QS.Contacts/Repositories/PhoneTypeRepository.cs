using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace QS.Contacts
{
	public static class PhoneTypeRepository
	{
		public static IList<PhoneType> GetPhoneTypes (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType> ().List<PhoneType> ();
		}
	}
}