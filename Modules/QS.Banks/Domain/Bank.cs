using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;

namespace QS.Banks.Domain
{
	[Appellative (
		Gender = GrammaticalGender.Masculine,
		Nominative = "банк",
		NominativePlural = "банки")]
	[EntityPermission]
	public class Bank: PropertyChangedBase, IValidatableObject, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название банка должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string bik;

		[StringLength (9, MinimumLength = 9, ErrorMessage = "Бик должен состоять из 9 цифр.")]
		[Display (Name = "БИК")]
		public virtual string Bik {
			get { return bik; }
			set { SetField (ref bik, value, () => Bik); }
		}

		IObservableList<CorAccount> corAccounts = new ObservableList<CorAccount>();

		public virtual IObservableList<CorAccount> CorAccounts {
			get { return corAccounts; }
			set {
				SetField(ref corAccounts, value, () => CorAccounts);
			}
		}
		
		public virtual IObservableList<CorAccount> ObservableCorAccounts => CorAccounts;
		
		CorAccount defaultCorAccount;

		[StringLength (25, MinimumLength = 20, ErrorMessage = "Номер корреспондентского счета должен содержать 20 цифр и не превышать 25-ти.")]
		[Display (Name = "К/С")]
		public virtual CorAccount DefaultCorAccount {
			get { return defaultCorAccount; }
			set { SetField (ref defaultCorAccount, value, () => DefaultCorAccount); }
		}

		string city;

		[Required (ErrorMessage = "Город является обязательным полем.")]
		[Display (Name = "Город")]
		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		BankRegion region;

		[Required (ErrorMessage = "Регион является обязательным полем.")]
		[Display (Name = "Регион")]
		[PropertyChangedAlso("RegionText")]
		public virtual BankRegion Region {
			get { return region; }
			set { SetField (ref region, value, () => Region); }
		}

		bool deleted;

		[Display (Name = "Удалён")]
		public virtual bool Deleted {
			get { return deleted; }
			set { SetField (ref deleted, value, () => Deleted); }
		}

		#endregion

		public Bank ()
		{
			Name = String.Empty;
			Bik = String.Empty;
			DefaultCorAccount = null;
			City = String.Empty;
		}

		public static bool EqualsWithoutId (Bank first, Bank second)
		{
			return (first.Bik == second.Bik &&
			first.City == second.City &&
			(first.DefaultCorAccount != null && first.DefaultCorAccount.CorAccountNumber == second.DefaultCorAccount.CorAccountNumber) &&
			first.CorAccounts.All(x => second.CorAccounts.Any(y => y.CorAccountNumber == x.CorAccountNumber)) &&
			first.Name == second.Name &&
			BankRegion.EqualsWithoutId(first.Region, second.Region) &&
			first.Deleted == second.Deleted);
		}

		public override bool Equals (Object obj)
		{
			Bank accountObj = obj as Bank; 
			if (accountObj == null)
				return false;
			else
				return Id.Equals (accountObj.Id);
		}

		public override int GetHashCode ()
		{
			return this.Id.GetHashCode (); 
		}

		#region IValidatableObject implementation

		public virtual String RegionText { get { return Region == null ? "" : 
				String.Format ("{0} {1} {2}", Region.RegionNum, Region.Region, Region.City); } }

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Bik))
				yield return new ValidationResult ("Бик должен содержать только цифры.", new[]{ "Bik" });
			if (DefaultCorAccount == null)
				yield return new ValidationResult ("Необходимо выбрать кор. счет по умолчанию", new[]{ nameof(DefaultCorAccount) });
		}

		#endregion
	}
}

