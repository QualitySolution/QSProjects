using System;
using System.Collections.Generic;
using QS.Contacts;
using QSOrmProject;
using QSOrmProject.DomainMapping;

namespace QSContacts
{
	public static partial class QSContactsMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static int MinSavePhoneLength = 7;

		public static string DefaultCityCode {
			get => ContactDefaults.DefaultCityCode;
			set => ContactDefaults.DefaultCityCode = value;
		}

		public static List<IOrmObjectMapping> GetModuleMaping ()
		{
			return new List<IOrmObjectMapping> {
				OrmObjectMapping<Post>.Create().DefaultTableView().SearchColumn("Должность", x => x.Name).End(),
				OrmObjectMapping<PhoneType>.Create().DefaultTableView().Column("Название", x => x.Name).End(),
				OrmObjectMapping<EmailType>.Create().DefaultTableView().Column("Название", x => x.Name).End()
			};
		}
	}
}

