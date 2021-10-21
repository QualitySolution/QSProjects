using QS.DomainModel.Entity;
using QS.Project.Domain;
using System.ComponentModel.DataAnnotations;

namespace QS.Report.Domain
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "выбранные пользователем принтеры",
		Nominative = "выбранный пользователем принтер")]
	public class UserSelectedPrinter : PropertyChangedBase, IDomainObject
	{
		private UserBase _user;
		private string _name;
		public virtual int Id { get; set; }

		[Display(Name = "Пользователь")]
		public virtual UserBase User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		[Display(Name = "Принтер")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
