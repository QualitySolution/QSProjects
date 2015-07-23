using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QSOrmProject;

namespace QSBanks
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		Nominative = "расчётный счет",
		NominativePlural = "расчётные счета"
	)]
	public class Account : PropertyChangedBase, IValidatableObject, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string number;

		[StringLength (25, MinimumLength = 20, ErrorMessage = "Номер банковского счета должен содержать 20 цифр и не превышать 25-ти.")]
		public virtual string Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		string code1c;

		public virtual string Code1c {
			get { return code1c; }
			set { SetField (ref code1c, value, () => Code1c); }
		}

		bool inactive;

		public virtual bool Inactive {
			get { return inactive; }
			set { SetField (ref inactive, value, () => Inactive); }
		}

		Bank inBank;

		[Required (ErrorMessage = "Банк должен быть заполнен.")]
		public virtual Bank InBank {
			get { return inBank; }
			set {SetField (ref inBank, value, () => InBank);
				Inactive = InBank == null || InBank.Deleted;
			}
		}

		#endregion

		bool isDefault;

		public virtual bool IsDefault {
			get { return isDefault; }
			set { SetField (ref isDefault, value, () => IsDefault); }
		}

		public virtual string BankName {
			get{
				if (InBank == null)
					return String.Empty;
				else
					return InBank.Name;
			}
		}

		public Account ()
		{
			Name = String.Empty;
			Number = String.Empty;
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Number))
				yield return new ValidationResult ("Номер счета может содержать только цифры.", new[]{ "Number" });
		}

		#endregion
	}
}

