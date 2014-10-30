using System;
using QSOrmProject;
using System.Collections.Generic;
using System.Data.Bindings;

namespace QSContacts
{
	[OrmSubjectAttibutes("Контактные лица")]
	public class Contact : BaseNotifyPropertyChanged, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Comment { get; set; }
		public virtual string Post { get; set; }
		public virtual Fired Fired { get; set; }
		public virtual IList<QSContacts.Phone> Phones { get; set; }
		public virtual IList<QSContacts.Email> Emails { get; set; }
		#endregion

		public Contact ()
		{
			Name = String.Empty;
			Comment = String.Empty;
			Post = String.Empty;
			Fired = Fired.no;
		}

		public override bool Equals(Object obj)
		{
			Contact contactObj = obj as Contact; 
			if (contactObj == null)
				return false;
			else
				return Id.Equals(contactObj.Id);
		}

		public override int GetHashCode()
		{
			return this.Id.GetHashCode(); 
		}
	}
	public enum Fired{
		[ItemTitleAttribute("Нет")] no,
		[ItemTitleAttribute("Да")] yes
	}

	public class FiredStringType : NHibernate.Type.EnumStringType
	{
		public FiredStringType() : base(typeof(Fired))
		{}
	}
}

