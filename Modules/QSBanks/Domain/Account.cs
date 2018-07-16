using System.Collections.Generic;
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
			get {
				if(string.IsNullOrEmpty(name)) {
					return "Основной";
				}
				return name; 
			}
			set { SetField (ref name, value, () => Name); }
		}

		string number;

		[StringLength (25, MinimumLength = 20, ErrorMessage = "Номер банковского счета должен содержать 20 цифр и не превышать 25-ти.")]
		[Display (Name = "Номер")]
		public virtual string Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		public virtual string Title{
			get{
				return "Счет: " + Name;
			}
		}
		string code1c;

		[Display (Name = "Код 1с")]
		public virtual string Code1c {
			get { return code1c; }
			set { SetField (ref code1c, value, () => Code1c); }
		}

		bool inactive;

		[Display (Name = "Неактивный")]
		public virtual bool Inactive {
			get { return inactive; }
			set { SetField (ref inactive, value, () => Inactive); }
		}

		Bank inBank;

		[Required (ErrorMessage = "Банк должен быть заполнен.")]
		[Display (Name = "Банк")]
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
			set {
				SetField(ref isDefault, value, () => IsDefault);
			}
		}

		/// <summary>
		/// Ссылка на владельца счета. Необходима для возможности установки счета по умолчанию.
		/// </summary>
		public virtual IAccountOwner Owner { get; set; }

		public Account ()
		{
		}
		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Number))
				yield return new ValidationResult ("Номер счета может содержать только цифры.", new[]{ "Number" });
		}

		#endregion
	}
}

