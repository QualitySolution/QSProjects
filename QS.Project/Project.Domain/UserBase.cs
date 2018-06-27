using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel;
using QSOrmProject;

namespace QS.Project.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "пользователи",
	Nominative = "пользователь")]
	public class UserBase : PropertyChangedBase, IDomainObject
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

		public UserBase()
		{
		}
	}
}
