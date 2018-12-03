using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;

namespace QSBanks
{
	[Appellative(NominativePlural = "Корреспондентский счет")]
	public class CorAccount : PropertyChangedBase, IValidatableObject, IDomainObject
	{
		public virtual int Id { get; set; }

		string corAccountNumber;

		[StringLength(25, MinimumLength = 20, ErrorMessage = "Номер кореспондентского счета должен содержать 20 цифр и не превышать 25-ти.")]
		[Display(Name = "К/С")]
		public virtual string CorAccountNumber {
			get { return corAccountNumber; }
			set { SetField(ref corAccountNumber, value, () => CorAccountNumber); }
		}

		Bank inBank;

		[Required(ErrorMessage = "Банк должен быть заполнен.")]
		[Display(Name = "Банк")]
		public virtual Bank InBank {
			get { return inBank; }
			set { SetField(ref inBank, value, () => InBank); }
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!new Regex(@"^[0-9]*$").IsMatch(CorAccountNumber))
				yield return new ValidationResult("Корреспондентский счет должен содержать только цифры.", new[] { nameof(CorAccountNumber) });
		}

		#endregion

		public bool EqualsWithoutId(CorAccount obj)
		{
			if(obj == null) {
				return false;
			}
			return CorAccountNumber == obj.CorAccountNumber &&
			InBank == obj.InBank;
		}

		public override bool Equals(object obj)
		{
			CorAccount compareCorAccount = obj as CorAccount;
			if(compareCorAccount == null) {
				return false;
			}
			return CorAccountNumber == compareCorAccount.CorAccountNumber &&
			Id == compareCorAccount.Id &&
			InBank.Id == compareCorAccount.InBank.Id;
		}

		public override int GetHashCode()
		{
			int result = 0;
			result += 31 * result + Id.GetHashCode();
			result += 31 * result + CorAccountNumber.GetHashCode();
			if(InBank != null) {
				result += 31 * result + InBank.Id.GetHashCode();
			}
			return result;
		}
	}
}
