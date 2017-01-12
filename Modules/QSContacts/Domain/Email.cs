using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace QSContacts
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	public class Email : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string address;

		[Display (Name = "Электронный адрес")]
		public virtual string Address {
		    get { return address; }
		    set { SetField (ref address, value, () => Address); }
		}

		private EmailType emailType;

		[Display (Name = "Тип адреса")]
		public virtual EmailType EmailType {
		    get { return emailType; }
		    set { SetField (ref emailType, value, () => EmailType); }
		}
			
	}
}

