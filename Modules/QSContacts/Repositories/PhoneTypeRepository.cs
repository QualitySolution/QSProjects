using QSOrmProject;
using System.Collections.Generic;

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