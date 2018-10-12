using System;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace QSContacts
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
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

