using System;
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
		private string _name;
		private DateTime? _created;
		private string _number;
		private string _code1C;
		private bool _inactive;
		private Bank _inBank;
		private CorAccount _bankCorAccount;
		private bool _isDefault;
		
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Name {
			get => string.IsNullOrEmpty(_name) ? "Основной" : _name;
			set => SetField (ref _name, value);
		}
		
		[Display (Name = "Создан")]
		public virtual DateTime? Created {
			get => _created;
			set => SetField (ref _created, value);
		}

		[Display (Name = "Номер")]
		public virtual string Number {
			get => _number;
			set => SetField (ref _number, value);
		}

		[Display (Name = "Код 1с")]
		public virtual string Code1c {
			get => _code1C;
			set => SetField (ref _code1C, value);
		}

		[Display (Name = "Неактивный")]
		public virtual bool Inactive {
			get => _inactive;
			set => SetField (ref _inactive, value);
		}

		[Display (Name = "Банк")]
		public virtual Bank InBank {
			get => _inBank;
			set => SetField (ref _inBank, value);
		}

		[Display(Name = "Кор. счет выбранного банка")]
		public virtual CorAccount BankCorAccount {
			get => _bankCorAccount;
			set => SetField(ref _bankCorAccount, value);
		}

		public virtual bool IsDefault {
			get => _isDefault;
			set {
				if(CanChangeIsDefault(value) && SetField(ref _isDefault, value) && value && Owner != null){
					foreach(var item in Owner.Accounts) {
						if(item != this) {
							item.IsDefault = false;
						}
					}
				}
			}
		}

		public virtual string Title => $"[{Id}] Счёт {Name} №{Number} в банке {InBank?.Name}";

		/// <summary>
		/// Ссылка на владельца счета. Необходима для возможности установки счета по умолчанию.
		/// </summary>
		public virtual IAccountOwner Owner { get; set; }

		public Account CreateCopy()
        {
			return (Account)this.MemberwiseClone();
        }

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

