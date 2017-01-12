using System;
using QSOrmProject;

namespace QSContacts
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "типы e-mail",
		Nominative = "тип e-mail")]
	public class EmailType : IDomainObject
	{
			#region Свойства
			public virtual int Id { get; set; }
			public virtual string Name { get; set; }
			#endregion

			public EmailType ()
			{
				Name = String.Empty;
			}
		}
}

