using System;
using QSOrmProject;

namespace QSPhones
{
	[OrmSubjectAttibutes("Телефоны")]
	public class Phone
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Number { get; set; }
		public virtual string Additional { get; set; }
		public virtual PhoneType NumberType { get; set; }
		#endregion

		public Phone()
		{
			Number = String.Empty;
			Additional = String.Empty;
		}
	}
}

