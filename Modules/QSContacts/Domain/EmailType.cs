using System;
using QSOrmProject;

namespace QSContacts
{
	[OrmSubject("Типы e-mail")]
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

