using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace QSContacts
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "должности",
		Nominative = "должность")]
	public class Post : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		private string name;

		[Display (Name = "Название")]
		public virtual string Name {
		    get { return name; }
		    set { SetField (ref name, value, () => Name); }
		}

		#endregion
		public Post()
		{
			Name = String.Empty;
		}
	}
}

