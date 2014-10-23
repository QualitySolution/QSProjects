using System;
using QSOrmProject;

namespace QSPhones
{
	[OrmSubjectAttibutes("Типы e-mail адресов")]
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

