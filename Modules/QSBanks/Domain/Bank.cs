using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace QSBanks
{
	[OrmSubject (JournalName = "Банки", DefaultJournalMode = ReferenceButtonMode.CanEdit | ReferenceButtonMode.TreatEditAsOpen)]
	public class Bank: PropertyChangedBase, IValidatableObject, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название банка должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string bik;

		[StringLength (9, MinimumLength = 9, ErrorMessage = "Бик должен состоять из 8 цифр.")]
		public virtual string Bik {
			get { return bik; }
			set { SetField (ref bik, value, () => Bik); }
		}

		string corAccount;

		[StringLength (25, MinimumLength = 20, ErrorMessage = "Номер кореспондентского счета должен содержать 20 цифр и не превышать 25-ти.")]
		public virtual string CorAccount {
			get { return corAccount; }
			set { SetField (ref corAccount, value, () => CorAccount); }
		}

		string city;

		[Required (ErrorMessage = "Город является обязательным полем.")]
		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		BankRegion region;

		[Required (ErrorMessage = "Регион является обязательным полем.")]
		public virtual BankRegion Region {
			get { return region; }
			set { SetField (ref region, value, () => Region); }
		}

		bool deleted;

		public virtual bool Deleted {
			get { return deleted; }
			set { SetField (ref deleted, value, () => Deleted); }
		}

		#endregion

		public Bank ()
		{
			Name = String.Empty;
			Bik = String.Empty;
			CorAccount = String.Empty;
			City = String.Empty;
		}

		public static bool EqualsWithoutId (Bank first, Bank second)
		{
			return (first.Bik == second.Bik &&
			first.City == second.City &&
			first.CorAccount == second.CorAccount &&
			first.Name == second.Name &&
			((first.Region == null && second.Region == null) || ( first.Region != null && first.Region.Equals (second.Region))) &&
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

		public virtual String GetRegionString { get { return Region == null ? "" : 
				String.Format ("{0} {1} {2}", Region.RegionNum, Region.Region, Region.City); } }

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Bik))
				yield return new ValidationResult ("Бик должен содержать только цифры.", new[]{ "Bik" });
			if (!new Regex (@"^[0-9]*$").IsMatch (CorAccount))
				yield return new ValidationResult ("Корреспондентский счет должен содержать только цифры.", new[]{ "CorAccount" });
		}

		#endregion
	}
}

