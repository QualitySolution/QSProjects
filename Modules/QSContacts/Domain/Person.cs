using System;
using QSOrmProject;
using QSProjectsLib;

namespace QSContacts
{
	[OrmSubject("Человек")]
	public class Person
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string PatronymicName { get; set; }
		public virtual string Lastname { get; set; } 
		#endregion

		public string NameWithInitials{
			get { return StringWorks.PersonNameWithInitials (Lastname, Name, PatronymicName);
			}
		}

		public Person ()
		{
			Name = String.Empty;
			PatronymicName = String.Empty;
			Lastname = String.Empty;
		}
	}
}

