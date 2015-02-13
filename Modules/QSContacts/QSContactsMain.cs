using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSContacts
{
	public static class QSContactsMain
	{

		public static List<OrmObjectMapping> GetModuleMaping()
		{
			return new List<OrmObjectMapping>
			{
				new OrmObjectMapping(typeof(Contact), typeof(ContactDlg), "{QSContacts.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] {"Surname", "Name", "Lastname", "Post"}),
				new OrmObjectMapping(typeof(PhoneType), null, "{QSContacts.PhoneType} Name[Название];"),
				new OrmObjectMapping(typeof(EmailType), null, "{QSContacts.EmailType} Name[Название];")
			};
		}
	}
}

