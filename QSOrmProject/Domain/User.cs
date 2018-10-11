using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace QSOrmProject.Domain
{
	[OrmSubject(Gender = GrammaticalGender.Masculine,
	NominativePlural = "пользователи",
	Nominative = "пользователь")]
	[Obsolete("Данный класс перенесен в библиотеку QS.Project и назван UserBase, после того как все проекты переведут сборку на новый класс, этот будет удален.")]
	public class User : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Имя пользователя")]
		[Required(ErrorMessage = "Имя пользователя должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string login;

		[Display(Name = "Логин")]
		[Required(ErrorMessage = "Логин пользователя должен быть заполнен.")]
		public virtual string Login {
			get { return login; }
			set { SetField(ref login, value, () => Login); }
		}

		#endregion

		public User()
		{
		}
	}
}
