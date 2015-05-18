using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSContacts
{
	public static partial class QSContactsMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static List<OrmObjectMapping> GetModuleMaping ()
		{
			return new List<OrmObjectMapping> {
				new OrmObjectMapping (typeof(PhoneType), null, "{QSContacts.PhoneType} Name[Название];"),
				new OrmObjectMapping (typeof(EmailType), null, "{QSContacts.EmailType} Name[Название];")
			};
		}
	}
}

