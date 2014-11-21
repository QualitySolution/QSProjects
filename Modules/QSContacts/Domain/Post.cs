using System;
using QSOrmProject;

namespace QSContacts
{
	[OrmSubjectAttributes("Должности")]
	public class Post : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion
		public Post()
		{
			Name = String.Empty;
		}
	}
}

