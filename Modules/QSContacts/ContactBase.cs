using System;
using System.Collections.Generic;

namespace QSContacts
{
	public abstract class ContactBase : IContact
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

		public ContactBase()
		{
		}
	}

	public interface IContact
	{
		IList<Contact> Contacts { get; set;}
	}
}

