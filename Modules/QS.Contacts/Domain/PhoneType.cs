using System;
using QS.DomainModel.Entity;

namespace QS.Contacts
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы телефонов",
		Nominative = "тип телефона")]
	public class PhoneType : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public PhoneType ()
		{
			Name = String.Empty;
		}
	}
}

