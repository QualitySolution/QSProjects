using System;
using System.Collections.Generic;
using QSOrmProject;
using QSOrmProject.DomainMapping;

namespace QSContacts
{
	public static partial class QSContactsMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static int MinSavePhoneLength = 7;

		public static List<IOrmObjectMapping> GetModuleMaping ()
		{
			return new List<IOrmObjectMapping> {
				OrmObjectMapping<PhoneType>.Create().DefaultTableView().Column("Название", x => x.Name).End(),
				OrmObjectMapping<EmailType>.Create().DefaultTableView().Column("Название", x => x.Name).End()
			};
		}
	}
}

