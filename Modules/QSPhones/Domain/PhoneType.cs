using System;
using QSOrmProject;

namespace QSPhones
{
	[OrmSubjectAttibutes("Типы телефонов")]
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

