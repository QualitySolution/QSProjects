using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSPhones
{
	public static class QSPhonesMain
	{

		public static List<OrmObjectMaping> GetModuleMaping()
		{
			return new List<OrmObjectMaping>
			{
				new OrmObjectMaping(typeof(PhoneType), null, "{QSPhones.PhoneType} Name[Название];"),
				new OrmObjectMaping(typeof(EmailType), null, "{QSPhones.EmailType} Name[Название];")
			};
		}
	}
}

