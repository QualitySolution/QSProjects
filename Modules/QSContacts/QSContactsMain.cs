using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSContacts
{
	public static class QSContactsMain
	{

		public static List<OrmObjectMaping> GetModuleMaping()
		{
			return new List<OrmObjectMaping>
			{
				new OrmObjectMaping(typeof(Contact), typeof(ContactDlg), "{QSContacts.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] {"Surname", "Name", "Lastname", "Post"}),
				new OrmObjectMaping(typeof(PhoneType), null, "{QSContacts.PhoneType} Name[Название];"),
				new OrmObjectMaping(typeof(EmailType), null, "{QSContacts.EmailType} Name[Название];")
			};
		}
	}
}

