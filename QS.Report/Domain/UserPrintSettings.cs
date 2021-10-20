using QS.DomainModel.Entity;
using QS.Project.Domain;
using System.ComponentModel.DataAnnotations;

namespace QS.Report.Domain
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "настройки печати для пользователя",
		Nominative = "настройки печати для пользователя")]
	public class UserPrintSettings : PropertyChangedBase, IDomainObject
	{
		private UserBase _user;
		private PageOrientationType _pageOrientation;
		private uint _numberOfCopies;

		public virtual int Id { get; set; }

		[Display(Name = "Пользователь")]
		public virtual UserBase User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		[Display(Name = "Ориентация")]
		public virtual PageOrientationType PageOrientation
		{
			get => _pageOrientation;
			set => SetField(ref _pageOrientation, value);
		}

		[Display(Name = "Количество копий")]
		public virtual uint NumberOfCopies
		{
			get => _numberOfCopies;
			set => SetField(ref _numberOfCopies, value);
		}

		public enum PageOrientationType
		{
			[Display(Name = "Книжная")]
			Portrait,

			[Display(Name = "Альбомная")]
			Landscape
		}
	}
}
