using System.Collections.Generic;
using QS.Contacts;
using QS.Deletion;

namespace QSContacts
{
	public static partial class QSContactsMain
	{
		public static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления в модуле Contacts...");

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(PhoneType),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "Тип телефона {1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<Phone> (item => item.NumberType)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Phone),
				SqlSelect = "SELECT id, number, additional FROM @tablename ",
				DisplayString = "Телефон {1} доб. {2}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(EmailType),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "Тип E-mail {1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<Email> (item => item.EmailType)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Email),
				SqlSelect = "SELECT id, address FROM @tablename ",
				DisplayString = "{1}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Person),
				SqlSelect = "SELECT id, surname, name FROM @tablename ",
				DisplayString = "{1} {2}"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Post),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}"
			}.FillFromMetaInfo ()
			);
		}
	}
}

