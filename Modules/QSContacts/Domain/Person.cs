using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSContacts
{
	[OrmSubject("Человек")]
	public class Person
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Surname { get; set; }
		public virtual string Lastname { get; set; } 
		#endregion

		public Person ()
		{
			Name = String.Empty;
			Surname = String.Empty;
			Lastname = String.Empty;
		}
	}
}

