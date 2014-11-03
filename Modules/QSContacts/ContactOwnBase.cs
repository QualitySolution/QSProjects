using System;
using System.Collections.Generic;

namespace QSContacts
{
	public abstract class ContactOwnBase : IContactOwn
	{
		private IList<Contact> contact;

		#region IContact implementation
		public virtual IList<Contact> Contacts {
			get {
				return contact;
			}
			set {
				contact = value;
			}
		}

		#endregion

		public ContactOwnBase()
		{
		}
	}

	public interface IContactOwn
	{
		IList<Contact> Contacts { get; set;}
	}
}

