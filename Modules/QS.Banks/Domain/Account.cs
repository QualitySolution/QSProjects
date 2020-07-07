using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace QS.Banks.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		Nominative = "расчётный счет",
		NominativePlural = "расчётные счета"
	)]
	[EntityPermission]
	public class Account : PropertyChangedBase, IValidatableObject, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;
		public virtual string Name {
			get => string.IsNullOrEmpty(name) ? "Основной" : name;
			set => SetField (ref name, value);
		}

		string number;
		[Display (Name = "Номер")]
		public virtual string Number {
			get => number;
			set => SetField (ref number, value);
		}

		string code1c;
		[Display (Name = "Код 1с")]
		public virtual string Code1c {
			get => code1c;
			set => SetField (ref code1c, value);
		}

		bool inactive;
		[Display (Name = "Неактивный")]
		public virtual bool Inactive {
			get => inactive;
			set => SetField (ref inactive, value);
		}

		Bank inBank;
		[Display (Name = "Банк")]
		public virtual Bank InBank {
			get => inBank;
			set { SetField (ref inBank, value);
				Inactive = InBank == null || InBank.Deleted;
			}
		}

		CorAccount bankCorAccount;
		[Display(Name = "Кор. счет выбранного банка")]
		public virtual CorAccount BankCorAccount {
			get => bankCorAccount;
			set => SetField(ref bankCorAccount, value);
		}

		bool isDefault;
		public virtual bool IsDefault {
			get => isDefault;
			set {
				if(SetField(ref isDefault, value) && value && Owner != null){
					foreach(var item in Owner.Accounts) {
						if(item != this) {
							item.IsDefault = false;
						}
					}
				}
			}
		}
		
		public virtual string Title => "Счет: " + Name;

		/// <summary>
		/// Ссылка на владельца счета. Необходима для возможности установки счета по умолчанию.
		/// </summary>
		public virtual IAccountOwner Owner { get; set; }
		
		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Number == null || !new Regex(@"^[0-9]*$").IsMatch(Number))
				yield return new ValidationResult("Номер счета не может быть пустым и должен содержать только цифры.", new[] { nameof(Number) });
			if(Number?.Length < 20 || Number?.Length > 25)
				yield return new ValidationResult("Номер банковского счета должен содержать 20 цифр и не превышать 25-ти.", new[] { nameof(Number) });
			if(InBank == null)
				yield return new ValidationResult("Банк должен быть заполнен.", new[] { nameof(InBank) });
			if(BankCorAccount == null || !BankCorAccount.InBank.Equals(InBank)) {
				yield return new ValidationResult("Должен быть выбран кор. счет выбранного банка.", new[] { nameof(BankCorAccount) });
			}
		}

		#endregion
	}
}

