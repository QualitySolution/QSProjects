using System;
using System.Collections.Generic;

namespace QSContacts
{
	public interface IContactOwner
	{
		IList<Contact> Contacts { get; set;}
	}
}

